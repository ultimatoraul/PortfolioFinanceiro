# 🏦 Portfolio Financeiro

## 📊 PortfolioFinanceiro.API

Trata-se de uma API dotNet 8.0 que oferece análises avançadas para portfólios de investimentos. 
O objetivo é fornecer insights sobre performance, risco e sugestões de rebalanceamento com base em dados pré-carregados.
A API conta com Swagger/OpenAPI configurado.

### Analytics Controller

1. **`GET /api/portfolios/{id}/performance`**
   - Retorna métricas de performance do portfólio

2. **`GET /api/portfolios/{id}/risk-analysis`**
   - Analisa risco e diversificação

3. **`GET /api/portfolios/{id}/rebalancing`**
   - Sugere ajustes para otimizar o portfólio

---

### Como Executar a API
Na pasta do Projeto PortfolioFinanceiro.API, execute o comando:
```bash
    `dotnet run`
```
*O profile default está configurado para rodar a API em https na porta 7250 https://localhost:7250.*

### Como Executar os Tests
Na pasta do Projeto PortfolioFinanceiro.Business.xUnit, execute o comando:
```bash
    `dotnet test`
```


## 📊 Dados Fornecidos

Projeto contém um SeedData.json pré configurado no projeto PortfolioFinanceiro.Data, podendo ser substituido para testar diferentes cenários. 
O formato deve ser mantido para garantir a compatibilidade com os endpoints.

A base atual está sendo alimentada por um **`SeedData.json`** com:
- **15 ativos** da bolsa brasileira (PETR4, VALE3, ITUB4, etc.)
- **3 portfólios** com diferentes estratégias (Conservador, Crescimento, Dividendos)
- **Histórico de preços** (30 dias) para 5 ativos principais
- **Market data** (Taxa Selic, Ibovespa)


## 🏗️ Estrutura da Solução

```
PortfolioFinanceiro.API
└── Controllers/
    └── AnalyticsController.cs
PortfolioFinanceiro.Business
├── DTO's
├── Interfaces
├── Models
└── Services/
    ├── PerformanceCalculator.cs
    ├── RiskAnalyzer.cs
    └── RebalancingOptimizer.cs
PortfolioFinanceiro.Business.xUnit
└── PerformanceCalculatorTests.cs
PortfolioFinanceiro.Data
├── DataContext.cs
├── PortfolioRepository.cs
└── SeedData.json
```

---

## 📋 Especificações dos Endpoints

### 1. Performance Analysis
**`GET /api/portfolios/{id}/performance`**

Retorna o PerformanceCalculator:

```json
{
  "totalInvestment": 100000.00,
  "currentValue": 108500.50,
  "totalReturn": 8.50,
  "totalReturnAmount": 8500.50,
  "annualizedReturn": 12.34,
  "volatility": 15.67,
  "positionsPerformance": [
    {
      "symbol": "PETR4",
      "investedAmount": 10000.00,
      "currentValue": 11200.00,
      "return": 12.00,
      "weight": 10.32
    }
  ]
}
```

---

### 2. Risk Analysis
**`GET /api/portfolios/{id}/risk-analysis`**

Retorna o RiskAnalyzer:

```json
{
  "overallRisk": "Medium",
  "sharpeRatio": 1.25,
  "concentrationRisk": {
    "largestPosition": {
      "symbol": "PETR4",
      "percentage": 25.5
    },
    "top3Concentration": 60.2
  },
  "sectorDiversification": [
    {
      "sector": "Energy",
      "percentage": 35.0,
      "risk": "High"
    }
  ],
  "recommendations": [
    "Reduzir exposição ao setor Energy (35%)",
    "Posição PETR4 representa 25.5% do portfólio (ideal < 20%)"
  ]
}
```

---

### 3. Rebalancing Suggestions
**`GET /api/portfolios/{id}/rebalancing`**

Retorna o RebalancingOptimizer:

```json
{
  "needsRebalancing": true,
  "currentAllocation": [
    {
      "symbol": "PETR4",
      "currentWeight": 25.5,
      "targetWeight": 20.0,
      "deviation": 5.5
    }
  ],
  "suggestedTrades": [
    {
      "symbol": "PETR4",
      "action": "SELL",
      "quantity": 50,
      "estimatedValue": 1775.00,
      "transactionCost": 5.33,
      "reason": "Reduzir de 25.5% para 20.0%"
    },
    {
      "symbol": "ITUB4",
      "action": "BUY",
      "quantity": 60,
      "estimatedValue": 1740.00,
      "transactionCost": 5.22,
      "reason": "Aumentar de 8.5% para 12.0%"
    }
  ],
  "totalTransactionCost": 10.55,
  "expectedImprovement": "Redução de 15% no risco de concentração"
}
```
---


## ❓ FAQ

Para essa solução foi utilizado o apoio da IA Claude Haiku 4.5, facilitando na construção dos Testes Unitários gerando mocks, no detalhamento da documentação, em um melhor entendimento das regras financeiras requisitadas.

### Comentários Adicionais:
Na Etapa Risk Analysis, tive dificuldade para determinar qual seria o calculo para determinar o overallRisk, então optei por uma classificação simples, em uma função em RiskFunctions.DetermineOverallRisk(), obtendo mais detalhes dos requisitos uma atualização será necessárias.

### Pontos a melhorar:
Evoluir a documentação do Swagger
Adicionar logs estruturados para debug dos cálculos
Testes de integração
Algoritmo de otimização de rebalanceamento avançado
Implementar a FinancialCalculator


---