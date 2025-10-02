import { Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';

@Injectable({ providedIn: 'root' })
export class FormValidationService {
  constructor(private fb: FormBuilder) {}

  createLoginForm(): FormGroup {
    return this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(1)]]
    });
  }

  createRegisterForm(): FormGroup {
    return this.fb.group({
      username: ['', [
        Validators.required, 
        Validators.minLength(3), 
        Validators.maxLength(50),
        this.usernameValidator
      ]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [
        Validators.required, 
        Validators.minLength(6),
        this.passwordStrengthValidator
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  createChangePasswordForm(): FormGroup {
    return this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [
        Validators.required, 
        Validators.minLength(6),
        this.passwordStrengthValidator
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  createUpdateProfileForm(currentUser?: any): FormGroup {
    return this.fb.group({
      username: [currentUser?.username || '', [
        Validators.required, 
        Validators.minLength(3), 
        Validators.maxLength(50),
        this.usernameValidator
      ]],
      email: [currentUser?.email || '', [Validators.required, Validators.email]]
    });
  }

  getFieldError(form: FormGroup, fieldName: string): string | null {
    const field = form.get(fieldName);
    if (!field || !field.touched || !field.errors) return null;

    const errors = field.errors;
    
    if (errors['required']) {
      return `${this.getFieldDisplayName(fieldName)} is required`;
    }
    if (errors['email']) {
      return 'Please enter a valid email address';
    }
    if (errors['minlength']) {
      const required = errors['minlength'].requiredLength;
      return `Minimum ${required} characters (entered: ${errors['minlength'].actualLength})`;
    }
    if (errors['maxlength']) {
      const required = errors['maxlength'].requiredLength;
      return `Maximum ${required} characters (entered: ${errors['maxlength'].actualLength})`;
    }
    if (errors['invalidUsername']) {
      return 'Username can only contain letters, numbers, and underscores';
    }
    if (errors['weakPassword']) {
      return 'Password must contain at least one digit and one letter';
    }
    if (errors['passwordMismatch']) {
      return 'Passwords do not match';
    }
    
    return 'Invalid value';
  }

  getFormErrors(form: FormGroup): string[] {
    const errors: string[] = [];
    
    Object.keys(form.controls).forEach(key => {
      const error = this.getFieldError(form, key);
      if (error) errors.push(error);
    });

    // Check form-level errors
    if (form.errors?.['passwordMismatch']) {
      errors.push('Passwords do not match');
    }

    return errors;
  }

  isFormValid(form: FormGroup): boolean {
    form.markAllAsTouched();
    return form.valid;
  }

  private getFieldDisplayName(fieldName: string): string {
    const names: { [key: string]: string } = {
      email: 'Email',
      password: 'Password',
      username: 'Username',
      currentPassword: 'Current password',
      newPassword: 'New password',
      confirmPassword: 'Confirm password'
    };
    return names[fieldName] || fieldName;
  }

  private usernameValidator(control: AbstractControl): { [key: string]: any } | null {
    const username = control.value;
    if (!username) return null;
    
    const validPattern = /^[a-zA-Z0-9_]+$/;
    if (!validPattern.test(username)) {
      return { invalidUsername: true };
    }
    
    return null;
  }

  private passwordStrengthValidator(control: AbstractControl): { [key: string]: any } | null {
    const password = control.value;
    if (!password) return null;
    
    const hasNumber = /\d/.test(password);
    const hasLetter = /[a-zA-Z]/.test(password);
    
    if (!hasNumber || !hasLetter) {
      return { weakPassword: true };
    }
    
    return null;
  }

  private passwordMatchValidator(group: AbstractControl): { [key: string]: any } | null {
    const password = group.get('newPassword')?.value || group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    
    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true };
    }
    
    return null;
  }
}