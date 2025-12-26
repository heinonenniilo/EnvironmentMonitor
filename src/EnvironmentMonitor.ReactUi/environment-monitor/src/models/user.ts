export interface User {
  email: string;
  loggedIn: boolean;
  roles: string[];
  authenticationProvider?: string;
}
