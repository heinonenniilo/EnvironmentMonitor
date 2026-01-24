export interface ApiKeyDto {
  id: string;
  description?: string;
  created: string;
  claims: ApiKeyClaimDto[];
}

export interface ApiKeyClaimDto {
  type: string;
  value: string;
}
