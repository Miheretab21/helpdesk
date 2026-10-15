export enum UserRole {
  Employee = 1,
  Agent = 2,
  Admin = 3,
}

export const UserRoleLabels: Record<UserRole, string> = {
  [UserRole.Employee]: 'Employee',
  [UserRole.Agent]: 'Agent',
  [UserRole.Admin]: 'Admin',
};

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
}
