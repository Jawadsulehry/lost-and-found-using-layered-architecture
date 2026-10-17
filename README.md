# Lost & Found Management System

A desktop application built for a Software Architecture course. It demonstrates **Layered Architecture** in C# using **.NET 8**, **Windows Forms** and **SQLite**.

Students and staff report lost and found items, and an Admin manages account approvals, handovers and returns, with notifications at every step.

---

## Table of Contents
1. [Features](#features)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [How the System Works](#how-the-system-works)
5. [Getting Started](#getting-started)
6. [Testing with Two Roles](#testing-with-two-roles)
7. [Technologies](#technologies)
8. [Limitations](#limitations)
9. [Group Members](#group-members)

---

## Features

**Accounts**
- New users register, which creates a *Pending* account request
- The Admin is notified and can approve or reject each request
- Only approved members can log in; pending and rejected accounts see a clear message
- Passwords are stored as SHA-256 hashes, never as plain text

**Members (students and staff)**
- Report a **Lost** or **Found** item (name, category, location, date, contact, description)
- View their own reports and their status
- Receive notifications (handover request, collect-your-item message)
- Search all items by keyword (other members' contact details are hidden)

**Admin**
- Notification inbox for every account request and every new Lost or Found report
- Approve or reject account requests
- Request handover from a finder, receive the item at the office, link it to the owner's lost report, and mark it returned
- Match suggestions: lost reports are ranked by category and shared keywords
- Live view of all items with status, reporter and contact details

---

## Architecture

The project uses **Layered Architecture**. Each layer only talks to the layer directly below it.

```
┌──────────────────────────────────────────────┐
│ PRESENTATION LAYER   (LostFound.UI)          │
│ LoginForm, RegisterForm, AdminDashboard,     │
│ MemberDashboard, LinkDialog                  │
└──────────────────────┬───────────────────────┘
                       │ calls only
┌──────────────────────▼───────────────────────┐
│ BUSINESS LOGIC LAYER (LostFound.BLL)         │
│ AuthService, AdminService, ItemService,      │
│ HandoverService, NotificationService         │
└──────────────────────┬───────────────────────┘
                       │ through interfaces
┌──────────────────────▼───────────────────────┐
│ DATA ACCESS LAYER    (LostFound.DAL)         │
│ IUserRepository, IItemRepository,            │
│ INotificationRepository + implementations    │
└──────────────────────┬───────────────────────┘
                       │ SQL
                ┌──────▼──────┐
                │   SQLite    │
                │ lostfound.db│
                └─────────────┘

LostFound.Models (User, Item, Notification) is shared by all layers.
```

| Layer | Project | Responsibility |
|---|---|---|
| Presentation | `LostFound.UI` | Windows Forms screens. Collects input and shows data. No business rules, no SQL. |
| Business logic | `LostFound.BLL` | Validation, password hashing, account approval, the item workflow, match scoring, creating notifications. |
| Data access | `LostFound.DAL` | The only code that knows SQL. Reads and writes SQLite through repository interfaces with parameterised queries. |
| Models | `LostFound.Models` | Plain data classes shared by all layers. |

### Why Layered architecture?
- **Separation of concerns:** each rule lives in one place instead of being spread across forms.
- **Enforced by the compiler:** the UI project has **no reference to the DAL**, so a form cannot touch the database directly.
- **Replaceable data store:** the BLL depends on repository interfaces, so SQLite can be swapped by changing only the DAL.
- **Easy to explain and test:** each layer has one job.

---

## Project Structure

```
LostFound.sln
├── LostFound.UI
│   ├── Program.cs
│   ├── LoginForm.cs
│   ├── RegisterForm.cs
│   ├── AdminDashboard.cs
│   ├── MemberDashboard.cs
│   └── LinkDialog.cs
├── LostFound.BLL
│   ├── AuthService.cs
│   ├── AdminService.cs
│   ├── ItemService.cs
│   ├── HandoverService.cs
│   └── NotificationService.cs
├── LostFound.DAL
│   ├── Database.cs
│   ├── IUserRepository.cs / UserRepository.cs
│   ├── IItemRepository.cs / ItemRepository.cs
│   └── INotificationRepository.cs / NotificationRepository.cs
└── LostFound.Models
    ├── User.cs
    ├── Item.cs
    └── Notification.cs
```

**Dependency rule:** `UI → BLL → DAL → Models` (UI also uses Models). The UI does not reference the DAL.

---

## How the System Works

### Account flow
```
Register (Pending) → Admin notified → Admin approves or rejects → approved members can log in
```

### Found item flow (status changes)
```
Found → AwaitingHandover → InOffice → Matched → Returned
```

1. A member reports a **Found** item and the Admin is notified.
2. Admin clicks **Request handover**. The finder is notified to hand the item in at the Admin Office.
3. Admin clicks **Received at office** once the item arrives.
4. Admin clicks **Link to lost report** and picks the owner's lost report. The owner is notified to collect the item.
5. Admin clicks **Mark as Returned**, which closes both linked reports.

The business layer blocks steps done out of order (for example, receiving an item before a handover was requested).

### Lost item flow
A member reports a **Lost** item, the Admin is notified, and the report waits until a matching found item is linked to it.

### Example: one click through the layers
When a member clicks **Send report**:

`MemberDashboard` (UI) → `ItemService.Report` (BLL: validates, creates the Admin notification) → `ItemRepository.Add` (DAL: runs the INSERT) → `lostfound.db`

---

## Getting Started

### Requirements
- Windows
- Visual Studio 2022 with the **.NET desktop development** workload
- .NET 8 SDK

### Run
1. Clone the repository:
   ```
   git clone https://github.com/Jawadsulehry/LostFound-Layered-Architecture.git
   ```
2. Open `LostFound.sln` in Visual Studio.
3. Right-click **LostFound.UI** and choose **Set as Startup Project**.
4. Press **F5**. The Login screen opens first.

The SQLite package restores automatically when the solution builds. The database file `lostfound.db` is created on first run.

### Default Admin login
| Username | Password |
|---|---|
| `admin` | `admin123` |

Change this password before using the system for anything real.

---

## Testing with Two Roles

Both windows read the same database, and dashboards refresh every 5 seconds.

1. Press **F5** for window 1 and log in as `admin`.
2. In Visual Studio, right-click **LostFound.UI** → **Debug** → **Start New Instance** for window 2.
3. In window 2, click **Create account** and register a member (for example `sara`).
4. In window 1, open **Account requests** and click **Approve account**.
5. In window 2, log in as the member and report an item.
6. In window 1, follow the handover steps on the **All items** tab.

---

## Technologies

| Area | Choice |
|---|---|
| Language | C# |
| Framework | .NET 8 |
| UI | Windows Forms (built in code, no designer files) |
| Database | SQLite via `Microsoft.Data.Sqlite` |
| Security | SHA-256 password hashing, parameterised SQL queries |
| IDE | Visual Studio 2022 |

---

## Limitations
- The database is a local file, so the app runs on one computer. Sharing across computers would need a server database.
- Notifications refresh by polling every 5 seconds instead of live push.
- Passwords use SHA-256. A production system should use a salted, slow hash such as PBKDF2 or bcrypt.
- The default Admin account has a fixed password that a real deployment must change.

---

## Group Members
- Member 1: _Your Name_
- Member 2: _Partner Name_

**Submitted to:** _Instructor Name_
