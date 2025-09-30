export const AUTH_CONFIG = {
  API_URL: 'http://localhost:5195/api/users',
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
    INVALID_CREDENTIALS: 'Неверный логин или пароль. Попробуйте еще раз.',
    NETWORK_ERROR: 'Ошибка сети. Проверьте соединение с интернетом.',
    UNKNOWN_ERROR: 'Произошла неизвестная ошибка при входе.'
  },
  REGISTER: {
    EMAIL_EXISTS: 'Пользователь с таким email уже существует.',
    WEAK_PASSWORD: 'Пароль слишком слабый.',
    INVALID_DATA: 'Некорректные данные регистрации.',
    UNKNOWN_ERROR: 'Произошла ошибка при регистрации.'
  },
  TOKEN: {
    INVALID: 'Недействительный токен авторизации.',
    EXPIRED: 'Сессия истекла. Войдите в систему заново.',
    NOT_FOUND: 'Токен авторизации не найден.'
  },
  VALIDATION: {
    REQUIRED: 'Поле обязательно для заполнения',
    EMAIL_INVALID: 'Введите корректный email адрес',
    PASSWORD_TOO_SHORT: 'Пароль должен содержать минимум 6 символов',
    USERNAME_TOO_SHORT: 'Имя пользователя должно содержать минимум 3 символа',
    USERNAME_INVALID: 'Имя пользователя может содержать только буквы, цифры и подчеркивания'
  }
} as const;

export const SUCCESS_MESSAGES = {
  LOGIN: 'Добро пожаловать в таверну! 🎉',
  REGISTER: 'Регистрация успешна! Теперь можете войти в таверну.',
  LOGOUT: 'Вы успешно вышли из системы.',
  PASSWORD_CHANGED: 'Пароль успешно изменен.',
  PROFILE_UPDATED: 'Профиль успешно обновлен.'
} as const;