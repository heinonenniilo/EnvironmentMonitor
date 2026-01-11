export interface AuthInfoCookie {
  loginState: boolean;
  errors: string[];
  errorCode?: string | null;
  timestamp: string;
}
