// Response shape of GET /api/Search/rates.
// Container properties are camelCased by ASP.NET's default serializer, while
// the source-modeled properties keep their [JsonPropertyName] snake_case names.
export interface Tin {
  type: string | null
  value: string | null
  business_name: string | null
}

export interface Provider {
  npi: number[] | null
  tin: Tin | null
}

export interface NegotiatedPrice {
  negotiated_rate: number | null
}

export interface ProviderGroupRate {
  providers: Provider[]
  negotiatedPrices: NegotiatedPrice[]
}

export interface Procedure {
  name: string | null
  billing_code: string | null
}

export interface ProcedureProviderGroupRates {
  procedure: Procedure | null
  groupRates: ProviderGroupRate[]
}
