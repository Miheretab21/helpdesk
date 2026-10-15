export enum UserRole {
  Employee = 1,
  Agent = 2,
  Admin = 3,
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
}