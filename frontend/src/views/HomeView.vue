<script setup lang="ts">
import { ref } from 'vue'

// Response shape of GET /api/Search/rates.
// Container properties are camelCased by ASP.NET's default serializer, while
// the source-modeled properties keep their [JsonPropertyName] snake_case names.
interface Tin {
  type: string | null
  value: string | null
  business_name: string | null
}

interface Provider {
  npi: number[] | null
  tin: Tin | null
}

interface NegotiatedPrice {
  negotiated_rate: number | null
}

interface ProviderGroupRate {
  providers: Provider[]
  negotiatedPrices: NegotiatedPrice[]
}

interface Procedure {
  name: string | null
  billing_code: string | null
}

interface ProcedureProviderGroupRates {
  procedure: Procedure | null
  groupRates: ProviderGroupRate[]
}

const billCode = ref('')
const providerNpi = ref('')
const providerEin = ref('')
const loading = ref(false)
const error = ref<string | null>(null)
const results = ref<ProcedureProviderGroupRates[] | null>(null)

const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

async function search() {
  const code = billCode.value.trim()
  if (!code) {
    error.value = 'Please enter a bill code.'
    return
  }

  const npi = providerNpi.value.trim()
  const ein = providerEin.value.trim()

  loading.value = true
  error.value = null
  results.value = null

  try {
    let uri = `/api/Search/rates?billCode=${encodeURIComponent(code)}`
    if (npi) uri += `&providerNpi=${encodeURIComponent(npi)}`
    if (ein) uri += `&providerEin=${encodeURIComponent(ein)}`

    const res = await fetch(uri)
    if (!res.ok) {
      // 400 returns a plain-text reason; 500 has no body.
      const body = await res.text()
      throw new Error(body || `Request failed (${res.status})`)
    }
    results.value = (await res.json()) as ProcedureProviderGroupRates[]
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Something went wrong.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="search">
    <h1>Rate Search</h1>

    <form class="search-form" @submit.prevent="search">
      <div class="fields">
        <div class="field">
          <label for="billCode">
            Bill Code <span class="req" aria-hidden="true">*</span>
          </label>
          <input
            id="billCode"
            v-model="billCode"
            type="text"
            placeholder="e.g. 10040"
            autocomplete="off"
            required="true"
            aria-required="true"
          />
        </div>

        <div class="field">
          <label for="providerNpi">
            Provider NPI <span class="optional">(optional)</span>
          </label>
          <input
            id="providerNpi"
            v-model="providerNpi"
            type="text"
            placeholder="e.g. 1234567890"
            autocomplete="off"
          />
        </div>

        <div class="field">
          <label for="providerEin">
            Provider EIN <span class="optional">(optional)</span>
          </label>
          <input
            id="providerEin"
            v-model="providerEin"
            type="text"
            placeholder="e.g. 12-3456789"
            autocomplete="off"
          />
        </div>
      </div>

      <button type="submit" :disabled="loading">
        {{ loading ? 'Searching…' : 'Search' }}
      </button>
    </form>

    <p v-if="loading" class="status">Loading rates… (this can take a while for large CMS files)</p>
    <p v-else-if="error" class="status error">{{ error }}</p>

    <template v-else-if="results">
      <p v-if="results.length === 0" class="status">No results for that search.</p>

      <section v-for="(item, i) in results" :key="i" class="procedure">
        <h2>
          {{ item.procedure?.name ?? 'Unknown procedure' }}
          <span class="code" v-if="item.procedure?.billing_code">
            ({{ item.procedure.billing_code }})
          </span>
        </h2>

        <div v-for="(group, g) in item.groupRates" :key="g" class="group">
          <div class="prices">
            <span v-for="(price, p) in group.negotiatedPrices" :key="p" class="price">
              {{ price.negotiated_rate != null ? currency.format(price.negotiated_rate) : '—' }}
            </span>
          </div>

          <ul class="providers">
            <li v-for="(provider, pr) in group.providers" :key="pr">
              <strong>{{ provider.tin?.business_name ?? 'Unknown provider' }}</strong>
              <span v-if="provider.tin?.value" class="ein">EIN: {{ provider.tin.value }}</span>
              <span v-if="provider.npi?.length" class="npi">
                NPI: {{ provider.npi.join(', ') }}
              </span>
            </li>
          </ul>
        </div>
      </section>
    </template>
  </main>
</template>

<style scoped>
.search {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.search-form {
  margin-bottom: 1.5rem;
}

.fields {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.field label {
  font-weight: 600;
  font-size: 0.9rem;
}

.req {
  color: #c0392b;
}

.optional {
  font-weight: 400;
  font-size: 0.85rem;
  opacity: 0.6;
}

.search-form input {
  width: 100%;
  box-sizing: border-box;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--color-border, #ccc);
  border-radius: 4px;
}

.search-form button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  background: hsla(160, 100%, 37%, 1);
  color: white;
  cursor: pointer;
}

.search-form button:disabled {
  opacity: 0.6;
  cursor: default;
}

.status {
  padding: 0.75rem 0;
}

.status.error {
  color: #c0392b;
}

.procedure {
  border: 1px solid var(--color-border, #e0e0e0);
  border-radius: 6px;
  padding: 1rem 1.25rem;
  margin-bottom: 1rem;
}

.procedure h2 {
  font-size: 1.1rem;
  margin: 0 0 0.75rem;
}

.procedure .code {
  font-weight: 400;
  opacity: 0.7;
}

.group {
  border-top: 1px solid var(--color-border, #eee);
  padding-top: 0.75rem;
  margin-top: 0.75rem;
}

.prices {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.price {
  font-weight: 700;
  background: hsla(160, 100%, 37%, 0.12);
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
}

.providers {
  list-style: none;
  padding: 0;
  margin: 0;
}

.providers li {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  padding: 0.25rem 0;
}

.ein,
.npi {
  opacity: 0.75;
  font-size: 0.9rem;
}
</style>
