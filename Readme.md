# Requisitos Funcionais — Score Clientes Adonai

## 1. Visão Geral

* **Objetivo:** Descrever o funcionamento do score de clientes.
* **Público-alvo do documento:** Product Owner, Analistas de Negócio, QA e Stakeholders de UX.

## 4. Requisitos Funcionais (RF)

### RF-001 — Calcular score mensal do cliente

O sistema deve calcular mensalmente o score operacional dos clientes com base nas métricas de desempenho logístico, com o objetivo de determinar sua elegibilidade para novos agendamentos no portal.

#### Pré-condições

* O cliente deve ter registros dentro do período especificado
* Os dados devem existir nas tabelas de histórico de agendamento
* O período de análise deve ser mensal

O score deve considerar as seguintes regras:

* Métricas de entrada:
  * Contagem de no-show
  * Atraso acumulado em segundos
  * Volume total de serviços
  * Valor faturado (receita)

Após a obtenção dos dados, será aplicada a fórmula de cálculo definida pelo modelo estatístico adotado pelo negócio (XGBoost), que irá gerar um score dinâmico de 0 a 1000. O resultado deve ser classificado para aplicação das regras de punição temporal.

Caso algum dos dados necessários esteja indisponível, o sistema deve aplicar o score fixo de 500 ao cliente.

| Fator                 | Tipo de Variável | Impacto no Score | Justificativa |
|----------------------|------------------|------------------|--------------|
| No-show              | Punição          | Alto peso negativo | Maior prejuízo operacional (ociosidade do slot) |
| Atraso acumulado     | Punição          | Peso negativo | Afeta o turnover e a agenda sequencial do pátio |
| Faturamento recebido | Amortecimento | Peso positivo | Prioriza clientes mais rentáveis para o negócio Adonai |
| Volume de serviços   | Bonificação | Peso positivo | Recompensa a recorrência e a parceria logística do cliente |

#### Cálculo do score
Formulá:

Isso significa:
* valor = Valor de faturamento recebido pelo cliente no mês alvo
* noshow = Média de “NO SHOW” praticado pelo cliente no mês alvo (𝑀é𝑑𝑖𝑎 =
𝑁ú𝑚𝑒𝑟𝑜 𝑑𝑒 𝑛𝑜𝑠ℎ𝑜𝑤𝑠 𝑛𝑜 𝑚ê𝑠 𝑎𝑙𝑣𝑜
𝑁ú𝑚𝑒𝑟𝑜 𝑑𝑒 𝑡𝑟𝑎𝑏𝑎𝑙ℎ𝑜𝑠 𝑡𝑜𝑡𝑎𝑙𝑖𝑧𝑎𝑑𝑜𝑠 𝑓𝑒𝑖𝑡𝑜𝑠 𝑝𝑒𝑙𝑜 𝑐𝑙𝑖𝑒𝑛𝑡𝑒 𝑛𝑜 𝑚ê𝑠 𝑎𝑙𝑣𝑜
)
* atraso = Número em segundos de atraso total no mês alvo
* servicos = Número de serviços quantificados no total feito pelo cliente ao longo do mês alvo

\[
\int_0^1 x^2 \, dx = \frac{1}{3}
\]
#### Critérios de aceite

* O sistema deve calcular o score mensalmente
* O score deve estar entre 0 e 1000
* O score deve estar vinculado ao cliente correto
* O score deve poder ser consultado posteriormente
* O cálculo deve considerar todas as variáveis definidas

---

### RF-002 – Classificar cliente por faixa de score

O sistema deve classificar o cliente em uma faixa de confiabilidade operacional com base no score calculado para o mês de referência. A classificação deve determinar o nível de confiabilidade do cliente e será utilizada para aplicar regras de restrição no agendamento de novos serviços.

#### Pré-condições

* O score deve ter sido calculado previamente (RF-001)
* O score deve estar disponível para consulta no sistema

O sistema deve classificar o cliente em três níveis de confiabilidade com base na pontuação recebida. Essa classificação será utilizada para determinar as restrições de agendamento aplicáveis ao cliente.

| Classificação | Faixa de Score | Nível de Confiança |
|-------------|-----------------|-------------------|
| Verde | Score ≥ 700 | Alta confiabilidade |
| Amarelo | 350 ≤ Score < 700 | Confiabilidade moderada |
| Vermelho | Score < 350 | Baixa confiabilidade |

#### Critérios de aceite

* O sistema deve classificar corretamente os clientes de acordo com a faixa de score
* Cada cliente deve possuir exatamente uma classificação por mês
* A classificação deve estar associada ao score calculado
* A classificação deve estar disponível para consulta e uso nas regras do sistema

---

### RF-003 – Identificar clientes com baixo volume de serviços (Low Volume)

O sistema deve identificar clientes que não possuem volume mínimo de serviços no período de análise e impedir que esses clientes tenham o score dinâmico calculado, atribuindo um score padrão até que possuam dados suficientes para avaliação estatística.

#### Pré-condições

* O cliente deve possuir registros no período analisado
* O sistema deve conseguir calcular o total de serviços realizados no mês
* O período de análise deve ser mensal

O sistema deverá validar o volume total de serviços realizados pelo cliente no mês de referência.

Caso o cliente não atinja o volume mínimo necessário para análise estatística:

* O score dinâmico não deve ser calculado
* O cliente deve receber score padrão (500 pontos)
* O cliente deve ser marcado como **low volume**
* O sistema deve continuar registrando os dados do cliente para avaliações futuras

#### Regras aplicadas

| Condição | Comportamento do sistema |
|----------|--------------------------|
| Cliente possui volume suficiente | Score calculado normalmente |
| Cliente não possui volume suficiente | Score dinâmico não calculado |
| Cliente com low volume | Recebe score padrão |
| Cliente com low volume | Deve ser reavaliado quando atingir volume mínimo |

#### Critérios de aceite

* O sistema deve identificar corretamente clientes com baixo volume
* Clientes low volume não devem ter score dinâmico calculado
* Clientes low volume devem receber score padrão
* O sistema deve permitir recalcular o score quando o cliente atingir volume suficiente
* O sistema deve manter o histórico de dados mesmo para clientes low volume

---

### RF-004 – Aplicar bloqueio de agendamento por score

O sistema deve aplicar restrições de agendamento para clientes com base na classificação do score mensal, determinando o período mínimo que o cliente deverá aguardar antes de realizar novos agendamentos.

O bloqueio deve ser aplicado automaticamente no momento da tentativa de agendamento.

#### Pré-condições

* O score do cliente deve ter sido calculado (RF-001)
* O cliente deve possuir uma classificação de score definida (RF-002)
* O cliente deve tentar realizar um novo agendamento

O sistema deve aplicar as seguintes regras:

| Classificação | Faixa Score | Bloqueio | Regra aplicada |
|-------------|--------------|-----------|----------------|
| Verde | Score ≥ 700 | 0 dias | Cliente pode agendar normalmente |
| Amarelo | 350 ≤ Score < 700 | 15 dias | Cliente só pode agendar após 15 dias |
| Vermelho | Score < 350 | 30 dias | Cliente só pode agendar após 30 dias |

#### Critérios de aceite

* Clientes verdes devem conseguir agendar normalmente
* Clientes amarelos não devem conseguir agendar antes de 15 dias
* Clientes vermelhos não devem conseguir agendar antes de 30 dias
* O sistema deve impedir agendamento em período bloqueado
* O sistema deve informar a data mínima disponível

---

### RF-005 – Validar disponibilidade final do calendário de agendamento

O sistema deve validar a disponibilidade final do calendário de agendamentos considerando simultaneamente as restrições de score do cliente e a disponibilidade real dos slots operacionais.

Essa validação deve garantir que apenas datas realmente elegíveis sejam apresentadas ao cliente no momento do agendamento.

#### Pré-condições

* O cliente deve possuir score calculado (RF-001)
* O cliente deve possuir classificação de score definida (RF-002)
* O sistema deve ter aplicado as regras de bloqueio (RF-004)
* O sistema deve possuir a agenda operacional disponível

#### Regras aplicadas

O sistema deve determinar as datas disponíveis para agendamento considerando:

* Período de bloqueio baseado no score do cliente
* Slots já ocupados
* Datas disponíveis na agenda operacional
* Regras operacionais existentes no sistema

| Situação | Comportamento do sistema |
|----------|--------------------------|
| Cliente elegível e slot disponível | Permitir agendamento |
| Cliente elegível mas slot ocupado | Não permitir agendamento |
| Cliente em período de bloqueio | Não exibir datas bloqueadas |
| Cliente fora do bloqueio | Exibir datas disponíveis normalmente |

O sistema deve considerar como data inicial de elegibilidade a primeira data possível após o término do período de bloqueio do cliente.

#### Critérios de aceite

* O sistema deve exibir apenas datas elegíveis para o cliente
* O sistema não deve permitir agendamento em datas bloqueadas por score
* O sistema não deve exibir slots já ocupados
* O sistema deve respeitar simultaneamente as regras de score e as regras operacionais
* O sistema deve recalcular a disponibilidade em tempo real ao consultar o calendário
* O sistema deve garantir consistência entre a data exibida e a data permitida para agendamento