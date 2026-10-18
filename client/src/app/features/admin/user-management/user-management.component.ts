import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, UserListItem, CreateUserRequest, UpdateUserRequest } from '../../../core/services/user.service';
import { UserRole, UserRoleLabels } from '../../../core/models/user.model';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule, SpinnerComponent],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss',
})
export class UserManagementComponent {
  private readonly userService = inject(UserService);
  
  protected readonly UserRole = UserRole;
  protected readonly UserRoleLabels = UserRoleLabels;

  // State
  protected readonly users = signal<UserListItem[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  
  // Modal state
  protected readonly showModal = signal(false);
  protected readonly isEditMode = signal(false);
  protected readonly modalTitle = computed(() => 
    this.isEditMode() ? 'Edit User' : 'Create New User'
  );
  
  // Form state
  protected readonly form = signal({
    id: '',
    email: '',
    fullName: '',
    password: '',
    role: UserRole.Employee,
  });

  protected readonly isSubmitting = signal(false);

  constructor() {
    this.loadUsers();
  }

  private async loadUsers() {
    this.loading.set(true);
    this.error.set(null);
    
    try {
      const users = await this.userService.list().toPromise();
      this.users.set(users || []);
    } catch (err) {
      this.error.set('Failed to load users');
      console.error(err);
    } finally {
      this.loading.set(false);
    }
  }

  protected openCreateModal() {
    this.isEditMode.set(false);
    this.form.set({
      id: '',
      email: '',
      fullName: '',
      password: '',
      role: UserRole.Employee,
    });
    this.showModal.set(true);
  }

  protected openEditModal(user: UserListItem) {
    this.isEditMode.set(true);
    this.form.set({
      id: user.id,
      email: user.email,
      fullName: user.fullName,
      password: '',
      role: user.role,
    });
    this.showModal.set(true);
  }

  protected closeModal() {
    this.showModal.set(false);
  }

  protected async submitForm() {
    const formData = this.form();
    
    if (!formData.email || !formData.fullName) {
      this.error.set('Email and Full Name are required');
      return;
    }

    this.isSubmitting.set(true);
    this.error.set(null);

    try {
      if (this.isEditMode()) {
        const updateRequest: UpdateUserRequest = {
          fullName: formData.fullName,
          role: formData.role,
        };
        
        if (formData.password) {
          updateRequest.password = formData.password;
        }

        await this.userService.update(formData.id, updateRequest).toPromise();
        this.error.set(null);
      } else {
        if (!formData.password) {
          this.error.set('Password is required for new users');
          this.isSubmitting.set(false);
          return;
        }

        const createRequest: CreateUserRequest = {
          email: formData.email,
          fullName: formData.fullName,
          password: formData.password,
          role: formData.role,
        };

        await this.userService.create(createRequest).toPromise();
      }

      this.showModal.set(false);
      await this.loadUsers();
    } catch (err: any) {
      this.error.set(err?.error?.message || 'Failed to save user');
      console.error(err);
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected async deleteUser(user: UserListItem) {
    if (!confirm(`Are you sure you want to delete ${user.fullName}?`)) {
      return;
    }

    this.isSubmitting.set(true);
    this.error.set(null);

    try {
      await this.userService.delete(user.id).toPromise();
      await this.loadUsers();
    } catch (err) {
      this.error.set('Failed to delete user');
      console.error(err);
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected updateFormField(field: string, value: any) {
    const currentForm = this.form();
    this.form.set({
      ...currentForm,
      [field]: value,
    });
  }

  protected onEmailChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.updateFormField('email', value);
  }

  protected onFullNameChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.updateFormField('fullName', value);
  }

  protected onPasswordChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.updateFormField('password', value);
  }

  protected onRoleChange(event: Event) {
    const value = parseInt((event.target as HTMLSelectElement).value, 10) as UserRole;
    this.updateFormField('role', value);
  }

  protected roleClass(role: UserRole): string {
    switch (role) {
      case UserRole.Admin:
        return 'role-badge--admin';
      case UserRole.Agent:
        return 'role-badge--agent';
      default:
        return 'role-badge--employee';
    }
  }
}
