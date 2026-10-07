export enum TicketStatus {
  New = 1,
  Assigned = 2,
  InProgress = 3,
  Resolved = 4,
  Closed = 5,
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
}

export interface Category {
  id: string;
  name: string;
}