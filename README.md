# 🚀 Simulador de Encomendas em Drone (dti digital)

> **Autor:** Davi Lucas do Carmo Nogueira
> **Data de Conclusão:** 29/09/2025
> **Tecnologia Principal:** C# (.NET Core)

## 🎯 Visão Geral do Projeto

Este projeto é um simulador de gerenciamento de frotas de drones urbanos, focado na otimização da alocação de pacotes para o menor número de viagens possível. O sistema modela entregas, drones e seus voos em um mapa 2D de coordenadas, implementando uma lógica de simulação orientada a eventos para refletir o ciclo de vida de uma entrega.

## ⚙️ Regras e Premissas Adotadas

Para a simulação, foram adotados os seguintes valores iniciais e regras:

| Parâmetro | Valor Inicial Adotado | Unidade | Descrição |
| :--- | :--- | :--- | :--- |
| Capacidade Máxima do Drone | **10** | kg | Peso máximo que um drone pode transportar. |
| Alcance Máximo do Drone | **20** | km | Distância máxima que um drone pode percorrer com uma única carga de bateria. |
| Velocidade Média do Drone | **1** | km/min | Usada para simular o tempo de voo. |
| Coordenada da Base (Hub) | **(0, 0)** | - | Ponto de partida e retorno de todos os drones. |
| Prioridades | `Baixa`, `Média`, `Alta` | - | Define a ordem de processamento dos pacotes. |

**Regras Essenciais:**
* Pacotes com peso superior à capacidade do drone são **rejeitados**.
* A distância entre dois pontos (X1, Y1) e (X2, Y2) é calculada usando a **Distância Euclidiana** Distância = √((x2 - x1)² + (y2 - y1)²).
* A lógica de alocação prioriza a **combinação de pacotes** que maximiza a utilização de capacidade e alcance por viagem.

## 💻 Arquitetura e Tecnologia

O projeto foi desenvolvido em **C#** utilizando o Framework: .NET 9.0 (SDK 9.0.304), seguindo uma arquitetura de separação de responsabilidades (ex: Domínio, Serviços, Testes). A lógica central é independente da interface.

* **Linguagem:** C#
* **Framework:** .NET 9.0 (SDK 9.0.304)
* **Interface:** Console Application (CLI) - *Fallback Garantido* / API RESTful (Opcional)

## 🌐 API REST (Opcional - Rascunho)

Se houver tempo, será criada uma API RESTful com os seguintes endpoints principais:

| Método | Endpoint | Descrição |
| --- | --- | --- |
| POST | /pedidos | Registrar novo pedido |
| GET | /drones/status | Obter status de todos os drones |
| GET | /entregas/rotas | Visualizar rota e status de uma entrega |

**Modelo de Pedido:**
{
  "x": 10,
  "y": 5,
  "peso": 4.5,
  "prioridade": "Alta"
}

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
    dotnet run --project NomeDoProjeto.CLI
    ```
    *O sistema iniciará no modo Terminal, aguardando comandos para adicionar pedidos e iniciar a simulação.*

## 🧪 Testes Unitários

### Como Rodar os Testes

Para garantir a robustez das regras de negócio, foram implementados testes unitários.

1.  Navegue até o diretório da solução principal.
2.  Execute o comando:
    ```bash
    dotnet test
    ```
    *O resultado mostrará a cobertura e o sucesso dos testes, focados principalmente na lógica de alocação e validação de regras.*

## 🧠 Modelo de Otimização e Algoritmo

O coração deste sistema reside no algoritmo de alocação, implementado na classe `GerenciadorDeFrota` (ou similar). O objetivo é resolver uma variação do **Problema da Mochila (Knapsack Problem)**, onde a "mochila" é o drone e os "itens" são os pacotes.

1.  **Priorização:** Pacotes são ordenados inicialmente por `Prioridade (Alta > Média > Baixa)` e, em seguida, por `Tempo de Chegada` (ou `Peso`, para tentar encaixar itens maiores primeiro).
2.  **Alocação:** Para cada pacote, o gerenciador tenta formar uma viagem (lote) com o **máximo de peso e o menor percurso total** (validando que o caminho total não exceda o alcance do drone).
3.  **Métrica de Otimização:** 
---

## ⭐ Registro de Prompts e Assistência de IA (Opcional)

Em linha com as práticas de desenvolvimento moderno e transparência, utilizei ferramentas de IA para auxiliar na estruturação de classes, cálculos e documentação.

| # | Tópico Auxiliado | Prompt Utilizado (Resumo) | Resposta Relevante Recebida |
| :---: | :--- | :--- | :--- |
| 1 | Estrutura e Documentação | Crie modelo de README e Estrutura C# para Simulador de Drone. | Modelo de README e sugestão de classes. |
| 2 | Cálculo de Rota | Como calcular a distância euclidiana entre dois pontos (X, Y) em C#? | Fórmula e implementação do método `CalcularDistancia()`. |
| 3 | Lógica de Otimização | ... | ... |
