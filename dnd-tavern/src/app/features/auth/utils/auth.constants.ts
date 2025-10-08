export const AUTH_CONFIG = {
  API_URL: '/api/users',
  TOKEN_KEY: 'tavern_token',
  JWT_CLAIMS: {
    NameIdentifier: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
    Name: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
    Email: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
    Role: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
  },
  VALIDATION: {
    USERNAME_MIN_LENGTH: 3,
    USERNAME_MAX_LENGTH: 50,
    PASSWORD_MIN_LENGTH: 6
  },
  NOTIFICATION_DURATIONS: {
    SUCCESS: 3000,
    ERROR: 5000,
    INFO: 3000,
    WARNING: 4000
  }
} as const;

export const ERROR_MESSAGES = {
  LOGIN: {
    INVALID_CREDENTIALS: 'Invalid username or password. Please try again.',
    NETWORK_ERROR: 'Network error. Please check your internet connection.',
    UNKNOWN_ERROR: 'An unknown error occurred during login.'
  },
  REGISTER: {
    EMAIL_EXISTS: 'A user with this email already exists.',
    WEAK_PASSWORD: 'The password is too weak.',
    INVALID_DATA: 'Invalid registration data.',
    UNKNOWN_ERROR: 'An error occurred during registration.'
  },
  TOKEN: {
    INVALID: 'Invalid authorization token.',
    EXPIRED: 'Session expired. Please log in again.',
    NOT_FOUND: 'Authorization token not found.'
  },
  VALIDATION: {
    REQUIRED: 'This field is required.',
    EMAIL_INVALID: 'Please enter a valid email address.',
    PASSWORD_TOO_SHORT: 'Password must be at least 6 characters long.',
    USERNAME_TOO_SHORT: 'Username must be at least 3 characters long.',
    USERNAME_INVALID: 'Username can only contain letters, numbers, and underscores.'
  }
} as const;

export const SUCCESS_MESSAGES = {
  LOGIN: 'Welcome to the tavern! 🎉',
  REGISTER: 'Registration successful! You can now enter the tavern.',
  LOGOUT: 'You have successfully logged out.',
  PASSWORD_CHANGED: 'Password changed successfully.',
  PROFILE_UPDATED: 'Profile updated successfully.'
} as const;