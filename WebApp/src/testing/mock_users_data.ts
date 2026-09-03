import { LoginRequest, RegisterRequest } from '../app/auth/models/auth.model';
import { UpdateUserRequest, UserDetails } from '../app/user/models/user.model';

export const MOCK_USERS: UserDetails[] = [
  { id: 'u1', firstName: 'Alice', lastName: 'Anderson', email: 'alice@cinematf.local', cardNumber: '4111111111111111', phoneNumber: '+381 60 123 4567' },
  { id: 'u2', firstName: 'Bob', lastName: 'Brown', email: 'bob@cinematf.local', cardNumber: '5500000000000004', phoneNumber: null },
  { id: 'u3', firstName: 'Admin', lastName: 'CineMATF', email: 'admin@cinematf.local', cardNumber: '340000000000009', phoneNumber: '+381 11 987 6543' },
];

export const MOCK_USER: UserDetails = MOCK_USERS[0];

export const MOCK_USERNAME = 'alice';

export const MOCK_USERS_BY_NAME: Record<string, UserDetails> = {
  alice: MOCK_USERS[0],
  bob: MOCK_USERS[1],
  admin: MOCK_USERS[2],
};

export const MOCK_UPDATE_USER_REQUEST: UpdateUserRequest = {
  firstName: 'Alice',
  lastName: 'Anderson',
  email: 'alice@cinematf.local',
  cardNumber: '4111111111111111',
  phoneNumber: '+381 60 999 0000',
};

export const MOCK_LOGIN_REQUEST: LoginRequest = {
  userName: 'alice',
  password: 'Passw0rd!',
};


export const MOCK_REGISTER_REQUEST: RegisterRequest = {
  firstName: 'Carol',
  lastName: 'Clark',
  userName: 'carol',
  email: 'carol@cinematf.local',
  password: 'Passw0rd!',
  cardNumber: '4222222222222222',
  phoneNumber: null,
};
