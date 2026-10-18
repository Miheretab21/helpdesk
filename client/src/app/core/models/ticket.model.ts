export enum TicketStatus {
  New = 1,
  Assigned = 2,
  InProgress = 3,
  Resolved = 4,
  Closed = 5,
  Cancelled = 6
}

export enum TicketPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Urgent = 4,
}

export interface TicketListItem {
  id: string;
  title: string;
  status: TicketStatus;
  priority: TicketPriority;
  assignedToId: string | null;
  createdById: string;
  createdAt: string;
  updatedAt: string;
  slaDueAt: string;
  escalatedAt: string | null;
}

export interface Category {
  id: string;
  name: string;
}

export interface TicketDetail {
  id: string;
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  categoryId: string;
  assignedToId: string | null;
  createdById: string;
  createdAt: string;
  updatedAt: string;
  slaDueAt: string;
  escalatedAt: string | null;
}

export interface TicketAttachment { id: string; fileName: string; contentType: string; sizeBytes: number; uploadedById: string; createdAt: string; }
export interface TicketAuditEntry { id: string; action: string; details: string | null; actorId: string | null; occurredAt: string; }
