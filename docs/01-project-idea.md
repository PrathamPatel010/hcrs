# HCRS (High-Concurrency Reservation System)

## Overview

HCRS is a reservation platform for limited-capacity physical experiences such as gaming stations, VR setups, and simulator units.

The system is built around one hard constraint:

> Multiple users may try to reserve the same physical slot at the same time, but only one must succeed.

That makes slot allocation, payment handling, expiry management, and real-time availability the core engineering concerns of the system.

---

## Problem Statement

In many booking systems, availability is treated as a soft check, the system reads availability, shows it to the user, and hopes the data is still accurate by the time they confirm.

That creates real problems:

- Two users may attempt to reserve the same slot at nearly the same time
- Payment can fail after a slot is temporarily held
- A user may abandon the payment screen and leave stale reservations behind
- Availability shown in the UI can become outdated when another user books first

HCRS is designed to handle these cases correctly.

It does this through:

- database-level row locking during reservation creation (SELECT WITH UPDLOCK, ROWLOCK, HOLDLOCK)
- time-bound reservation expiry via background jobs, server-side, not client-dependent
- explicit state machines for every key entity, invalid transitions are impossible
- real-time availability pushed to connected clients via SignalR, no polling
- idempotent payment webhook handling, safe even if the webhook fires twice
- signed QR codes for tamper-proof entry validation

---

## System Roles

### Admin

- Reviews and approves or rejects owner onboarding (KYC) requests
- Reviews and approves or rejects experience center requests
- Manages users and centers across the platform
- Runs contention simulations to verify reservation engine correctness
- Monitors system-level metrics

### Owner

- Completes KYC onboarding (approved by admin before access is granted)
- Creates and manages experience centers (approved by admin before going live)
- Configures units, pricing, and availability windows
- Generates slots from availability windows
- Validates customer entry at the venue via QR scan or booking ID
- Views business analytics for their centers

### Customer

- Discovers nearby experience centers via location-based search
- Views live slot availability (updated in real time)
- Creates a temporary reservation (5-minute window to complete payment)
- Completes payment via Razorpay
- Receives a QR-based entry pass on successful payment

---

## Core Features

---

### Location-Based Discovery

Customers explore nearby experience centers using location-based search.

- Current location detection (browser geolocation)
- Manual location search
- Radius-based filtering
- Map view and list view
- Filter by date and experience type

Center locations are stored as geographic coordinates (latitude, longitude) in SQL Server and queried using spatial distance functions.

---

### Owner Onboarding (KYC)

Owners must complete KYC before they can create experience centers.

#### Flow

- Owner registers and submits KYC details
- Admin reviews the submission
- Admin approves or rejects with a reason
- Rejected owners may resubmit

#### OwnerStatus state machine

```
KYC_NOT_SUBMITTED → PENDING_REVIEW → APPROVED
                                   └→ REJECTED → PENDING_REVIEW
```

An owner cannot create centers until their status is APPROVED.

---

### Experience Center Management

Owners create and manage experience centers. Each center must be approved by an admin before customers can discover or book it.

#### Flow

- Owner creates a center (DRAFT status)
- Owner submits it for approval (DRAFT → PENDING_APPROVAL)
- Admin approves or rejects with a reason
- Rejected centers return to DRAFT for revision and resubmission

#### CenterStatus state machine

```
DRAFT → PENDING_APPROVAL → ACTIVE
                       └→ REJECTED → DRAFT
```

Centers in DRAFT or PENDING_APPROVAL are not visible to customers.

---

### Unit and Slot Configuration

Each experience center contains one or more units (e.g., individual gaming stations, VR pods).

#### Units

- Owner creates units under a center
- Each unit has a type, capacity, and pricing
- Units can be enabled or disabled

#### Slots

- Owner defines availability windows (days of week, time ranges)
- System pre-generates individual time slots from those windows
- Each slot has a start time, end time, and belongs to a unit
- SlotStatus defaults to AVAILABLE on generation

Slots are pre-generated rather than computed on demand. This allows atomic locking at the database row level during reservation.

---

### Reservation Engine

The reservation engine is the core of HCRS. It handles the hardest problem: multiple users attempting to book the same slot simultaneously.

#### Reservation flow

1. Customer selects a slot and clicks Book Now
2. System opens a transaction
3. System acquires a row-level lock on the slot (SELECT WITH UPDLOCK, ROWLOCK, HOLDLOCK)
4. System checks slot status, must be AVAILABLE
5. If available: creates reservation (PENDING), updates slot (AVAILABLE → HELD), commits transaction
6. If not available: transaction rolls back, request fails cleanly, no double booking
7. Hangfire schedules an expiry job for 5 minutes from now
8. Customer is redirected to payment

#### After payment success

- Reservation → CONFIRMED
- Slot → BOOKED

#### If reservation expires (Hangfire job fires)

- Checks reservation is still PENDING (idempotent, safe to run twice)
- Reservation → EXPIRED
- Slot → AVAILABLE
- Availability update broadcast to connected clients

#### If customer cancels

- Reservation → CANCELLED
- Slot → AVAILABLE
- Availability update broadcast

#### Concurrency guarantee

Two users clicking Book Now on the same slot at the same millisecond will both hit the reservation endpoint. The database lock serializes these requests. Exactly one succeeds. The other receives a clean failure response. No double booking is possible.

#### ReservationStatus state machine

```
PENDING → CONFIRMED
       └→ EXPIRED
       └→ CANCELLED
```

PENDING means: reservation exists, payment not yet complete, expiry window still active.

#### SlotStatus state machine

```
AVAILABLE → HELD → BOOKED
             └→ AVAILABLE
```

HELD means: slot is temporarily unavailable while an active PENDING reservation exists.

---

### Payment Flow

Payments are processed via Razorpay (sandbox environment).

#### Flow

1. On reservation creation, backend creates a Razorpay order and returns the order ID + key to the client
2. Client opens Razorpay checkout popup
3. Customer completes payment
4. Razorpay fires a webhook to the backend
5. Backend verifies HMAC signature on the webhook payload, rejects if invalid
6. Backend processes the payment outcome and updates reservation + slot state accordingly
7. Webhook handler is idempotent, safe if Razorpay fires the same webhook twice

#### Payment outcomes

**Success**

- Payment → SUCCESS
- Reservation → CONFIRMED
- Slot → BOOKED
- QR code generated
- Confirmation email sent

**Failure**

- Payment → FAILED
- Reservation remains PENDING until Hangfire expires it

**Cancel (customer closes popup)**

- Payment → CANCELLED
- Reservation → CANCELLED
- Slot → AVAILABLE

**Timeout (customer never pays)**

- Hangfire expiry job fires
- Reservation → EXPIRED
- Slot → AVAILABLE

#### PaymentStatus state machine

```
PENDING → SUCCESS
       └→ FAILED
       └→ CANCELLED
```

The backend never trusts the client for payment confirmation. All outcomes are driven by the Razorpay webhook.

---

### Real-Time Availability

Slot availability is pushed to all connected clients using SignalR the moment it changes.

#### When updates are broadcast

- Reservation created (slot AVAILABLE → HELD)
- Payment succeeds (slot HELD → BOOKED)
- Reservation expires (slot HELD → AVAILABLE)
- Reservation cancelled (slot HELD → AVAILABLE)

#### Implementation

- Clients join a SignalR group specific to the center they are viewing
- Updates are broadcast only to the relevant center's group, not globally
- Clients reconnect and rejoin their group automatically on disconnection
- This eliminates stale availability in the UI without any polling

---

### QR Code and Entry Validation

On payment success, a QR code is generated and issued to the customer as their entry pass.

#### QR payload

The QR code encodes a signed payload:

```
bookingId + HMAC signature
```

The payload is signed using a server-side secret. This prevents forged QR codes, a QR code cannot be fabricated without the secret.

#### Validation checks (in order)

1. Verify HMAC signature, reject immediately if invalid
2. Reservation exists and status is CONFIRMED
3. Payment status is SUCCESS
4. Current time falls within the reserved slot window
5. Slot has not already been used (prevents re-entry)

If all checks pass: entry is granted and slot is marked as used.

#### Validation methods

- QR scan (camera-based, in-browser)
- Manual booking ID entry (fallback)

---

### Notification System

A background notification pipeline handles user-facing communication.

Notifications are fully decoupled from the reservation transaction. They are dispatched as Hangfire background jobs after the transaction commits. A notification failure never affects the reservation outcome.

#### Notifications

- Reservation confirmation email (on CONFIRMED)
- Payment result email (success and failure)
- Booking reminder email (before slot time)

---

### Owner Analytics

Owners have access to business-level operational data for their centers.

#### Metrics

- Total revenue (overall, per center, per unit)
- Booking count (overall, by time period)
- Slot utilisation rate per unit
- Peak usage periods (hour of day, day of week)
- Experience-level performance comparison

All analytics queries use Dapper and stored procedures for performance. Date range filtering is supported on all metrics.

---

### Admin Management

Admins manage the platform at a system level.

- User listing with filters (role, status, join date)
- User deactivation and reactivation
- Center listing across all owners
- Consolidated approval queues (KYC + center approvals)
- System overview metrics (total users, centers, reservations, revenue)

---

### Contention Simulation

Admins can run contention tests against the live reservation engine to verify correctness under concurrent load.

#### How it works

- Admin selects a slot and specifies a number of concurrent attempts (e.g. 50)
- Simulator fires all requests simultaneously using parallel tasks
- System processes all requests through the real reservation engine
- Results show: how many succeeded, how many failed, the winning reservation ID, slot final state, time taken

#### What it proves

- Only one reservation succeeds regardless of concurrency level
- Failed requests fail cleanly with no partial state
- No double booking occurs
- Slot state remains consistent throughout

---

## Key Design Decisions

### Reservation correctness over eventual consistency

Slot allocation uses pessimistic locking at the database level. This guarantees correctness at the cost of some throughput under extreme concurrency. For a system where double booking is unacceptable, this is the right tradeoff.

### Server-side expiry

Reservation expiry is handled by Hangfire on the server. It does not depend on the client staying connected or making any further request. A user can close their browser and the reservation will still expire correctly.

### Signed QR codes

QR payloads include an HMAC signature. This means a valid QR code cannot be constructed without the server secret, even if someone knows the booking ID format.

### Idempotent webhook handling

Razorpay may deliver the same webhook more than once. The webhook handler checks whether the payment has already been processed before applying any state changes. Running it twice produces the same result as running it once.

### Decoupled notifications

Notifications are dispatched after the transaction commits, as background jobs. The reservation engine never waits for an email to send. A failed email service cannot affect booking correctness.

### Hybrid data access

EF Core handles all transactional writes where correctness and entity lifecycle management matter. Dapper handles all read-heavy queries, analytics, discovery, availability, where performance and projection control matter more than the ORM.

---

## Technology Stack

| Area             | Technology            |
| ---------------- | --------------------- |
| Backend API      | ASP.NET Core          |
| ORM              | Entity Framework Core |
| Query Layer      | Dapper                |
| Database         | SQL Server            |
| Cache            | Redis                 |
| Background Jobs  | Hangfire              |
| Real-Time        | SignalR               |
| Payment          | Razorpay (sandbox)    |
| Validation       | FluentValidation      |
| Mapping          | Mapster               |
| Logging          | Serilog               |
| Containerisation | Docker                |
| Frontend         | Angular               |

---

## What This Project Demonstrates

HCRS is built to solve real engineering problems that occur in production reservation systems.

It demonstrates:

- preventing double booking under true concurrent load using database-level locking
- designing and enforcing explicit state machines across multiple entities
- handling payment lifecycle including failures, cancellations, and webhook idempotency
- pushing real-time state changes to connected clients without polling
- releasing held resources server-side without any client involvement
- generating tamper-proof entry passes using signed payloads
- separating side effects (notifications) from core transactions
- using a hybrid data access strategy, transactional writes via EF Core, optimised reads via Dapper
- building a full Clean Architecture backend with proper layer separation
- containerising a full-stack application with Docker (deployment to a live URL is optional, dependent on available cloud credits)
