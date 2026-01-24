export interface ApiKeyDto {
  id: string;
  description?: string;
  created: string;
  enabled: boolean;
  claims: ApiKeyClaimDto[];
}

export interface ApiKeyClaimDto {
  type: string;
  value: string;
}

export interface UpdateApiKeyRequest {
  enabled?: boolean;
  description?: string;
}
