# 🚀 Simulador de Encomendas em Drone (dti digital)

> **Autor:** Davi Lucas do Carmo Nogueira
> **Data de Conclusão:** 29/09/2025
> **Tecnologia Principal:** C# (.NET Core)

## 🎯 Visão Geral do Projeto

Este projeto é um simulador de gerenciamento de frotas de drones urbanos, focado na otimização da alocação de pacotes para o menor número de viagens possível. O sistema modela entregas, drones e seus voos em um mapa 2D de coordenadas, implementando uma lógica de simulação orientada a eventos para refletir o ciclo de vida de uma entrega.

## ⚙️ Regras e Premissas Adotadas (Atualizado)

Para a simulação, foram adotados os seguintes valores iniciais e regras:

| Parâmetro            | Valor Adotado  | Unidade      | Descrição |
|---------------------|---------------|-------------|-----------|
| Malha da Cidade     | 50×50         | Coordenadas | O mapa 2D da cidade opera em uma malha quadriculada de 50×50 (do (0,0) ao (50,50)). |
| Coordenada da Base (Hub) | (0, 0)    | -           | Ponto de partida e retorno de todos os drones. |
| Velocidade Média do Drone | 60       | km/h (1 km/min) | Usada para simular o tempo de voo. |
| Prioridades         | Baixa, Média, Alta | -       | Define a ordem de processamento dos pacotes. |

**Regras Essenciais:**
* Pacotes com peso superior à capacidade do drone são **rejeitados**.
* A distância entre dois pontos (X1, Y1) e (X2, Y2) é calculada usando a **Distância Euclidiana** Distância = √((x2 - x1)² + (y2 - y1)²).
* A lógica de alocação prioriza a **combinação de pacotes** que maximiza a utilização de capacidade e alcance por viagem.

## ✈️ Frota de Drones e Capacidades

A frota de drones inicial é composta por 3 unidades, com capacidades distintas:

| Drone  | Capacidade Máx. (kg) | Alcance Máx. Base (km) |
|--------|----------------------|-------------------------|
| Pumba  | 18,0                 | 141,4                   |
| Mufasa | 10,0                 | 141,4                   |
| Simba  | 8,0                  | 141,4                   |

## 🔋 Alcance Condicional ao Peso (Regra de Negócio Crítica)

O alcance máximo teórico do drone é **141,4 km** (distância máxima na malha 50×50 - baseada na Distância Euclidiana).  
Porém, o **Alcance Efetivo** da rota é condicionado ao peso do pacote, refletindo a física da bateria e carga.

| Peso do Pacote | Alcance Efetivo |
|----------------|----------------|
| Até 3 kg       | 100% do Alcance Máximo |
| Até 6 kg       | 80% do Alcance Máximo  |
| Até 10 kg      | 70% do Alcance Máximo  |
| Acima de 10 kg | 60% do Alcance Máximo  |

### **Regras Essenciais:**
- **Distância:** calculada por **Distância Euclidiana**
- **Rejeição Lógica:**  
Pacotes com peso superior à capacidade do drone ou cuja rota exceda o Alcance Efetivo são rejeitados e mantidos na **fila pendente**.
- **Otimização:**  
A lógica de alocação prioriza a combinação de pacotes que **maximiza a utilização de capacidade e alcance por viagem**.

---
  
## 💻 Arquitetura e Tecnologia

O projeto foi desenvolvido em **C#** utilizando o Framework: .NET 8.0, seguindo uma arquitetura de separação de responsabilidades (ex: Domínio, Serviços, Testes). A lógica central é independente da interface.

* **Linguagem:** C#
* **Framework:** .NET 8.0 (LTS)
* **Interface:** Console Application (CLI) - *Fallback Garantido* / API RESTful (Opcional)

  A solução segue uma **arquitetura em camadas**, baseada em princípios simplificados de **Domain-Driven Design (DDD)**.  

| Projeto | Propósito | Conteúdo Principal |
| :------ | :-------- | :---------------- |
| **DroneDelivery.Domain** | Lógica de negócio central (Domínio) | Modelos, Entidades, Enums, Interfaces, Serviços de Otimização e Cálculo |
| **DroneDelivery.CLI** | Ponto de entrada para execução (linha de comando) | `Program.cs` inicializa frota, pedidos e executa a simulação |
| **DroneDelivery.Tests** | Garantia de qualidade do sistema | Testes unitários para regras e cálculos |

---
## 🧱 Entidades e Modelos Centrais

### **1. Ponto (Struct)**
Representa uma coordenada no mapa (X, Y).  
- Usado para localizar **base**, **clientes** e **drones**.  
- Implementado como `struct` por eficiência e simplicidade.

---

### **2. Prioridade (Enum)**
Define a importância de um pedido para **ordenar a alocação** no `GerenciadorDeFrota`.  
Valores:
- **Baixa**
- **Média**
- **Alta**

---

### **3. Pedido (Entidade)**
O pacote a ser entregue.

**Regra Validada:**
- Lança uma `ArgumentException` se o peso ≤ 0, garantindo integridade de dados na criação do pedido.

---

### **4. Drone (Entidade)**
Representa o agente de entrega.

**Atributos principais:**
- `CapacidadeMaxKg`
- `AlcanceMaxKm`
- `Status` *(Idle, EmVoo, Retornando)*

---

### **5. Voo (Entidade)**
Representa uma viagem planejada e em execução.  
Responsabilidades:
- Contém a lista de pacotes.
- Calcula o **PesoTotalCarga**.
- Determina a **DistanciaTotalRotaKm**.

---

## 🧮 Serviços e Algoritmos

### **1. ICalculadoraDistancia / CalculadoraEuclidiana**
- Define contrato para cálculo de rota.
- Implementação padrão: **Distância Euclidiana**. 

---

### **2. IGerenciadorDeFrota / GerenciadorDeFrota**
O núcleo de **otimização e alocação de pedidos**.

**Algoritmo Utilizado: Heurística Gulosa (Greedy)**  
1. Ordena pedidos por **Prioridade** e, depois, por **Peso** (maior primeiro).  
2. Itera sobre drones disponíveis e tenta adicionar pedidos à rota.  
3. Simula a rota completa (`Base → Entregas → Base`):
 - Se a distância total exceder o `AlcanceMaxKm`, o pacote é rejeitado.

#### **Processo de Alocação:**
- Itera sobre os drones disponíveis.
- Tenta adicionar pedidos à rota, respeitando:
1. **Capacidade** (`CapacidadeMaxKg`).
2. **Alcance Efetivo**.

---

### **3. ISimuladorDeVoos / SimuladorDeVoos**
Gerencia o **ciclo de vida dos voos**:
- Muda drones para `EmVoo` durante entregas.
- Retorna drones para `Idle` ao final do voo.
- Permite avançar o tempo da simulação.

> Antes de confirmar, **simula a rota completa** (`Base → Entregas → Base`) para validar o alcance com base no peso total transportado.

### **3. ISimuladorDeVoos / SimuladorDeVoos**
Gerencia:
- Ciclo de vida dos voos.
- Avanço do tempo na simulação (3 minutos).
- Mudança de status dos drones.

---

### **Etapas do Algoritmo:**
1. **Priorização:**
 - Ordenação dos pedidos por `Prioridade`.
 - Em empate, ordenação por **Peso** (maior primeiro).

2. **Alocação Gulosa:**
 - Iteração sobre os drones disponíveis.
 - Inclusão do próximo melhor pacote, respeitando restrições.

3. **Checagem Dupla de Restrições:**
 - **Capacidade:** peso do pacote não pode ultrapassar `CapacidadeMaxKg`.  
 - **Alcance:** a rota simulada não pode exceder o **Alcance Efetivo**.

---

## 🌐 API REST (Opcional - Rascunho)

Se houver tempo, será criada uma API RESTful com os seguintes endpoints principais:

| Método | Endpoint | Descrição |
| --- | --- | --- |
| POST | /pedidos | Registrar novo pedido |
| GET | /drones/status | Obter status de todos os drones |
| GET | /entregas/rotas | Visualizar rota e status de uma entrega |

**Modelo de Pedido:**

```json
{
  "x": 10,
  "y": 5,
  "peso": 4.5,
  "prioridade": "Alta"
}
```
## 🛠️ Como Executar o Projeto

### Pré-requisitos
- SDK do .NET 9.0 (versão utilizada no projeto: 9.0.304)

### Execução via Linha de Comando (CLI)

1.  Clone o repositório:
    ```bash
    git clone https://github.com/davilucas156/TestePratico-DTI-2025-DaviLucas
    cd TestePratico-DTI-2025-DaviLucas
    ```
2.  Execute o projeto principal (o *entry point* da aplicação):
    ```bash
    dotnet run --project DroneDelivery.CLI

    ```
    *O sistema iniciará no modo Terminal, aguardando comandos para adicionar pedidos e iniciar a simulação.*
    

# 🧪 Testes Unitários

Foram criados **testes unitários** para validar as principais regras de negócio do simulador, garantindo que o comportamento do sistema esteja de acordo com as premissas definidas.

Os testes foram implementados utilizando **xUnit**, e focam principalmente na lógica de alocação, cálculo de distância e validação dos pedidos.

---

## Estrutura de Testes

| Área Testada                 | Objetivo |
|------------------------------|----------|
| Cálculo de Distância         | Validar se a distância entre dois pontos (coordenadas) está correta. |
| Validação de Pedidos          | Confirmar que pedidos inválidos (peso acima da capacidade, valores nulos, etc.) são rejeitados. |
| Alocação de Pacotes em Drones | Garantir que os pacotes sejam alocados corretamente, respeitando capacidade e prioridade. |

---

## Como Executar os Testes

1. Certifique-se de que o **SDK .NET 9.0** está instalado na sua máquina.
2. Abra o terminal na raiz do projeto.
3. Execute o comando abaixo para rodar todos os testes:

```bash
dotnet test
```
exemplo de saida esperada 
```bash
Test run for DroneDelivery.Tests.dll (.NETCoreApp,Version=v9.0)
✔ Passed 10 tests
Total tests: 10. Passed: 10. Failed: 0. Skipped: 0.
````

## 🧠 Modelo de Otimização e Algoritmo

O coração deste sistema reside no algoritmo de alocação, implementado na classe `GerenciadorDeFrota`.  
O objetivo é resolver uma variação do **Problema da Mochila (Knapsack Problem)**, onde a "mochila" representa o drone e os "itens" são os pacotes a serem entregues.

### Etapas do Algoritmo

1. **Priorização:**  
   - Os pedidos são ordenados inicialmente por **Prioridade** na seguinte ordem:
     ```
     Alta > Média > Baixa
     ```
   - Em caso de empate na prioridade, a ordenação ocorre por **Peso** (maior para menor), para tentar encaixar primeiro os pacotes mais pesados.

2. **Alocação:**  
   - O gerenciador percorre a lista de drones disponíveis.
   - Para cada drone, tenta adicionar o **máximo de pedidos possíveis** respeitando:
     - **Capacidade máxima de carga (kg)**
     - **Alcance máximo do drone (km)**  
   - Antes de confirmar cada pacote, o sistema **simula a rota completa** (Base → Entregas → Base).  
     - Caso a **Distância Total** ultrapasse o alcance máximo, o pacote é rejeitado para aquela viagem.

3. **Métrica de Otimização:**  
   - Maximizar o aproveitamento da capacidade de cada drone.
   - Reduzir o número total de viagens necessárias para entregar todos os pedidos.
   - Garantir que todas as restrições de peso e alcance sejam atendidas.

---

**Resumo do Processo:**
> Este algoritmo segue uma abordagem **Gulosa (Greedy)**, sempre escolhendo o próximo melhor pacote disponível, de acordo com prioridade e peso, até que não seja mais possível incluir novos pacotes na viagem sem violar as restrições definidas.


## ⭐ Registro de Prompts e Assistência de IA (Opcional)

Em linha com as práticas de desenvolvimento moderno e transparência, utilizei ferramentas de IA para auxiliar na estruturação de classes, cálculos e documentação.

| # | Tópico Auxiliado | Prompt Utilizado (Resumo) | Resposta Relevante Recebida |
| :---: | :--- | :--- | :--- |
| 1 | planejamento | Extrai o case do pdf enviado pela DTI utilizando notebookLM (AI) para previnir alucinações mediante ao problema proposto, gerando um prompt para utilizar na IA que iria me auxiliar no desenvolvimento | prompt fiel ao documento evidenciando pontos vitais e bem explicativo. |
| 1.1 | tecnologia e estratégia | expliquei minha lógica para resolver o problema e pedi sugestões de tecnologias para conseguir entregar minha ideia baseado no prompt inicial, gerado pelo NotebookLM onde continha as informações necessárias para a entrega do case. | C#/.net
| 1.2 | Planejamento organizacional | "considere que tenho apenas sabado e domingo de 7hrs as 21hrs para desenvolver o case, me dê um planejamento de horários e tarefas considerando pausas programadas em formato quadro kanban." | planejamento |
| 2 | Estrutura e Documentação | "Crie modelo de README e Estrutura C# para o teste proposto." | Modelo de README e sugestão de classes. |
| 3 | Cálculo de Rota | Como calcular a distância entre dois pontos (X, Y) em C#? | Fórmula e lógica Euclidiana |
| 4 | Lógica de Otimização | Quero que cada drone carregue o máximo de acordo com a capacidade e prioridade otimizando viagens | Otimização Gulosa |
| 5 | Estrutura/arquitetura | Me ensine a construir uma estrutura de pastas para esse projeto | Arquitetura em camadas |
| 6 | Simulação de tempo | Crie a lógica de simulação orientada a eventos, com "ticks" de tempo para processar voos em andamento e mudar o status dos drones. | SimuladorDeVoos
