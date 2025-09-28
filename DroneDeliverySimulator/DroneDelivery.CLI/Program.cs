using DroneDelivery.Domain.Entities;
using DroneDelivery.Domain.Enums;
using DroneDelivery.Domain.Models;
using DroneDelivery.Domain.Services;
using System.Text;

const int LIMITE_CIDADE_X = 50;
const int LIMITE_CIDADE_Y = 50;
const double TEMPO_CICLO_SIMULACAO_MINUTOS = 3.0;

var calculadora = new CalculadoraEuclidiana();

var frota = new List<Drone>
{
    new Drone("Mufasa", capacidadeMaxKg: 10.0, velocidade: 60.0),
    new Drone("Simba", capacidadeMaxKg: 8.0, velocidade: 60.0),
    new Drone("Pumba", capacidadeMaxKg: 18.0, velocidade : 60.0)
};

var gerenciador = new GerenciadorDeFrota(calculadora, frota);
var simulador = new SimuladorDeVoos(frota);

var pedidosPendentes = new List<Pedido>();

ExecutarInterfaceCLI();

void ExecutarInterfaceCLI()
{
    Console.Title = "Simulador de Entregas em Drone (CLI)";
    ExibirCabecalho();

    while (true)
    {
        ExibirStatusAtual();
        ExibirMenu();

        var input = Console.ReadLine()?.ToUpperInvariant().Trim() ?? string.Empty;

        try
        {
            switch (input)
            {
                case "1":
                    AdicionarPedidoInterativo();
                    break;
                case "2":
                    IniciarSimulacao();
                    break;
                case "3":
                    MostrarStatusFrota();
                    break;
                case "0":
                    Console.WriteLine("\nSaindo do simulador. Até mais!");
                    return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Comando inválido. Tente novamente.");
                    Console.ResetColor();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nERRO: {ex.Message}");
            Console.WriteLine("Por favor, tente novamente com valores válidos.");
            Console.ResetColor();
        }

        Console.WriteLine("\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
    }
}

void ExibirCabecalho()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("=================================================");
    Console.WriteLine("     SIMULADOR DTI - GERENCIADOR DE FROTA ");
    Console.WriteLine("=================================================");
    Console.ResetColor();
    Console.Write($"Frota Ativa: {frota.Count} drones. ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("Capacidades:");
    Console.ResetColor();

    foreach (var drone in frota)
    {
        Console.WriteLine($"  -> {drone.Nome}: Capacidade {drone.CapacidadeMaxKg:F1}kg, Bateria {drone.NivelBateriaPercentual:F1}%, Velocidade {drone.VelocidadeMediaKmh:F1} km/h.");
    }

    // Regra de Alcance Variável (Manter por ser regra de negócio crítica)
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("-------------------------------------------------");
    Console.WriteLine($"* Alcance máximo de rota é {Drone.ALCANCE_MAXIMO_ROTA_KM:F1} km (100% da malha 50x50).");
    Console.WriteLine("  O **Alcance Efetivo** varia conforme o peso da carga:");
    Console.WriteLine("    - Até 3 kg: 100% de Alcance Efetivo.");
    Console.WriteLine("    - Até 6 kg: 80% de Alcance Efetivo.");
    Console.WriteLine("    - Até 10 kg: 70% de Alcance Efetivo.");
    Console.WriteLine("    - Acima de 10 kg: 60% de Alcance Efetivo.");
    Console.ResetColor();
}

void ExibirExplicacaoMatematica()
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n=== REGRAS DE CÁLCULO ===");
    Console.ResetColor();

    Console.WriteLine("1. **Distância (km):** 1 unidade no mapa = 1 km real. Usa **Distância Euclidiana**.");
    Console.WriteLine("2. **Tempo (min):** Calculado com base na Distância Total (Ida e Volta) e na Velocidade do Drone.");
    Console.WriteLine("=================================");
}

void ExibirMenu()
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("\n--- MENU DE COMANDOS ---");
    Console.ResetColor();
    Console.WriteLine(" [1] ADICIONAR NOVO PEDIDO");
    Console.WriteLine(" [2] INICIAR SIMULAÇÃO (Alocar pedidos pendentes e processar voos)");
    Console.WriteLine(" [3] MOSTRAR STATUS DA FROTA");
    Console.WriteLine(" [0] SAIR");
    Console.Write("\nDigite sua opção: ");
}

void ExibirStatusAtual()
{
    int voosEmAndamento = simulador.GetVoosEmAndamento().Count;

    Console.WriteLine("\n-------------------------------------------------");
    Console.Write($" Pedidos Pendentes: ");
    Console.ForegroundColor = pedidosPendentes.Count > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
    Console.Write(pedidosPendentes.Count);
    Console.ResetColor();

    Console.Write($" |  Voos em Andamento: ");
    Console.ForegroundColor = voosEmAndamento > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
    Console.Write(voosEmAndamento);
    Console.ResetColor();
    Console.WriteLine("\n-------------------------------------------------");
}

void AdicionarPedidoInterativo()
{
    Console.Clear();
    ExibirCabecalho();

    ExibirExplicacaoMatematica();
    DesenharMapaDaCidade();

    bool continuarAdicionando = true;

    do
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n-- NOVO PEDIDO --");
        Console.ResetColor();

        double peso;
        while (true)
        {
            Console.Write("Peso do Pacote (kg): ");
            if (double.TryParse(Console.ReadLine(), out peso) && peso > 0)
            {
                var capacidadeMaxima = frota.Max(d => d.CapacidadeMaxKg);
                if (peso > capacidadeMaxima)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ ERRO: O peso ({peso:F1}kg) excede a capacidade máxima de qualquer drone ({capacidadeMaxima:F1}kg). Tente um peso menor.");
                    Console.ResetColor();
                    continue;
                }
                break;
            }
            Console.WriteLine("Valor inválido. Insira um peso positivo.");
        }

        int x;
        while (true)
        {
            Console.Write($"Coordenada X (0 a {LIMITE_CIDADE_X}): ");
            if (int.TryParse(Console.ReadLine(), out x) && x >= 0 && x <= LIMITE_CIDADE_X) break;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Valor inválido. Coordenada X deve ser entre 0 e {LIMITE_CIDADE_X}.");
            Console.ResetColor();
        }

        int y;
        while (true)
        {
            Console.Write($"Coordenada Y (0 a {LIMITE_CIDADE_Y}): ");
            if (int.TryParse(Console.ReadLine(), out y) && y >= 0 && y <= LIMITE_CIDADE_Y) break;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Valor inválido. Coordenada Y deve ser entre 0 e {LIMITE_CIDADE_Y}.");
            Console.ResetColor();
        }

        Prioridade prioridade;
        string nomePrioridade = string.Empty;

        while (true)
        {
            Console.Write("Prioridade (A=Alta, M=Média, B=Baixa): ");
            var pInput = Console.ReadLine()?.ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(pInput) || pInput.Length != 1)
            {
                Console.WriteLine("Entrada inválida. Digite apenas A, M ou B.");
                continue;
            }

            switch (pInput)
            {
                case "A": nomePrioridade = "Alta"; break;
                case "M": nomePrioridade = "Media"; break;
                case "B": nomePrioridade = "Baixa"; break;
                default:
                    Console.WriteLine("Prioridade inválida. Use A, M ou B.");
                    continue;
            }

            if (Enum.TryParse(nomePrioridade, out prioridade)) break;
            Console.WriteLine($"Erro interno. O enum '{nomePrioridade}' não foi encontrado. Verifique a definição de Prioridade.");
        }

        var novoPedido = new Pedido(
            new Ponto(x, y),
            peso,
            prioridade
        );

        pedidosPendentes.Add(novoPedido);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n✅ Pedido ID {novoPedido.Id.ToString()[..4]} (Peso: {peso:F1}kg, Destino: ({x},{y})) ADICIONADO aos pendentes.");
        Console.ResetColor();

        while (true)
        {
            Console.WriteLine("\n-------------------------------------------------");
            Console.WriteLine("Deseja adicionar outro pedido ou voltar ao menu?");
            Console.WriteLine(" [C] Continuar (Adicionar outro pedido)");
            Console.WriteLine(" [F] Finalizar e voltar ao Menu Principal");
            Console.WriteLine("-------------------------------------------------");
            Console.Write("Opção: ");

            var cInput = Console.ReadLine()?.ToUpperInvariant().Trim();

            if (cInput == "F")
            {
                continuarAdicionando = false;
                break;
            }
            else if (cInput == "C")
            {
                Console.Clear();
                ExibirCabecalho();
                break;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Opção inválida. Use C para Continuar ou F para Finalizar.");
                Console.ResetColor();
            }
        }

    } while (continuarAdicionando);

}

void IniciarSimulacao()
{
    Console.Clear();
    ExibirCabecalho();

    if (!pedidosPendentes.Any() && !simulador.GetVoosEmAndamento().Any())
    {
        Console.WriteLine("\nNão há pedidos pendentes nem voos em andamento para iniciar a simulação.");
        return;
    }

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("\n-- INICIANDO CICLO DE SIMULAÇÃO --");
    Console.ResetColor();

    // 1. ALOCAÇÃO: O Gerenciador aplica a lógica Gulosa e retorna novos voos
    var novosVoos = gerenciador.AlocarPedidos(pedidosPendentes);

    // Remove os pedidos alocados da lista pendente local
    pedidosPendentes.RemoveAll(p => !gerenciador.PedidosPendentes.Contains(p));

    if (novosVoos.Any())
    {
        Console.WriteLine($"\n📦 {novosVoos.Count} novo(s) Voo(s) PLANEJADO(s) e INICIADO(s)!");

        // 2. INICIAR VOOS no Simulador
        simulador.IniciarVoos(novosVoos);
    }

    // Feedback de pacotes pendentes
    if (pedidosPendentes.Any())
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n⚠️ {pedidosPendentes.Count} Pacotes ficaram pendentes. Motivo(s) da falha:");
        Console.ResetColor();

        foreach (var pedido in pedidosPendentes)
        {
            var resultadoAnalise = gerenciador.AnalisarFalhaDeAlocacao(pedido);

            Console.Write($"  -> Pedido ID {pedido.Id.ToString()[..4]} (Peso: {pedido.Peso:F1}kg, Destino: ({pedido.LocalizacaoCliente.X}, {pedido.LocalizacaoCliente.Y})): ");

            if (resultadoAnalise.StartsWith("❌")) Console.ForegroundColor = ConsoleColor.Red;
            else Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(resultadoAnalise);
            Console.ResetColor();
        }
    }


    // 3. PROCESSAR TEMPO (Simulador de Voos)
    Console.WriteLine($"\nProcessando {TEMPO_CICLO_SIMULACAO_MINUTOS:F1} minuto(s) de simulação...");
    simulador.ProcessarCicloDeSimulacao(tempoDecorridoMinutos: TEMPO_CICLO_SIMULACAO_MINUTOS);


    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\n✅ Ciclo de Simulação CONCLUÍDO. Use [3] para ver o status da Frota.");
    Console.ResetColor();
}

void MostrarStatusFrota()
{
    Console.Clear();
    ExibirCabecalho();
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("\n-- STATUS ATUAL DA FROTA --");
    Console.ResetColor();

    var voosEmAndamento = simulador.GetVoosEmAndamento();

    foreach (var drone in frota)
    {
        var corStatus = drone.Status == DroneStatus.Idle ? ConsoleColor.Green : ConsoleColor.Yellow;

        Console.Write($"\n{drone.Nome}: ");
        Console.ForegroundColor = corStatus;
        Console.Write(drone.Status);
        Console.ResetColor();

        Console.Write($" | Bateria: ");
        Console.ForegroundColor = drone.NivelBateriaPercentual > 25 ? ConsoleColor.Yellow : ConsoleColor.Red;
        Console.Write($"{drone.NivelBateriaPercentual:F1}%");
        Console.ResetColor();

        var vooAssociado = voosEmAndamento.FirstOrDefault(v => v.DroneAlocado == drone);

        if (vooAssociado != null)
        {
            double tempoRestante = vooAssociado.TempoTotalEstimadoMinutos - vooAssociado.TempoDecorridoNoVooMinutos;
            if (tempoRestante < 0) tempoRestante = 0;

            Console.WriteLine($" - Localização: ({drone.LocalizacaoAtual.X:F1}, {drone.LocalizacaoAtual.Y:F1})");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"  ⏱️ Tempo Restante: {tempoRestante:F1} min (Total: {vooAssociado.TempoTotalEstimadoMinutos:F1} min)");
            Console.ResetColor();
        }
        else if (drone.Status == DroneStatus.Carregando)
        {
            var tempoRestanteTotal = drone.CiclosDeRecargaRestantes * TEMPO_CICLO_SIMULACAO_MINUTOS;
            Console.WriteLine($" - Recarregando... Faltam {drone.CiclosDeRecargaRestantes} ciclos ({tempoRestanteTotal:F1} min).");
        }
        else
        {
            Console.WriteLine(" - Pronto na Base.");
        }
    }
}


void DesenharMapaDaCidade()
{
    Console.WriteLine("\n🗺️  MAPA DA CIDADE (Malha 50x50)");

    var posicoesDrones = frota.Where(d => d.Status != DroneStatus.Idle)
                              .Select(d => new { Ponto = d.LocalizacaoAtual, Nome = d.Nome[0] })
                              .ToList();

    var posicoesPedidos = pedidosPendentes
                              .Select(p => new { Ponto = p.LocalizacaoCliente, ID = p.Id.ToString()[0] })
                              .ToList();

    const int FATOR_ESCALA = 5;
    const int NUMERO_CELULAS = LIMITE_CIDADE_X / FATOR_ESCALA;

    for (int yCelula = NUMERO_CELULAS - 1; yCelula >= 0; yCelula--)
    {
        Console.Write($" {yCelula * FATOR_ESCALA:D2} | ");

        for (int xCelula = 0; xCelula < NUMERO_CELULAS; xCelula++)
        {
            char celula = '.';
            int xMin = xCelula * FATOR_ESCALA;
            int xMax = xMin + FATOR_ESCALA;
            int yMin = yCelula * FATOR_ESCALA;
            int yMax = yMin + FATOR_ESCALA;
            ConsoleColor cor = ConsoleColor.White;

            if (xCelula == 0 && yCelula == 0)
            {
                celula = 'B';
                cor = ConsoleColor.Blue;
            }

            foreach (var dronePos in posicoesDrones)
            {
                if (dronePos.Ponto.X >= xMin && dronePos.Ponto.X < xMax &&
                    dronePos.Ponto.Y >= yMin && dronePos.Ponto.Y < yMax)
                {
                    celula = dronePos.Nome;
                    cor = ConsoleColor.Yellow;
                    goto CheckPedidos; // Sai do loop de drones para evitar sobrescrever a base
                }
            }

        CheckPedidos:
            if (cor != ConsoleColor.Yellow)
            {
                foreach (var pedidoPos in posicoesPedidos)
                {
                    if (pedidoPos.Ponto.X >= xMin && pedidoPos.Ponto.X < xMax &&
                        pedidoPos.Ponto.Y >= yMin && pedidoPos.Ponto.Y < yMax)
                    {
                        celula = 'P';
                        cor = ConsoleColor.Red;
                        break;
                    }
                }
            }

            Console.ForegroundColor = cor;
            Console.Write($"{celula} ");
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    Console.Write("    -");
    for (int x = 0; x < NUMERO_CELULAS; x++)
    {
        Console.Write("--");
    }
    Console.WriteLine("----");

    Console.Write("     ");
    for (int x = 0; x < NUMERO_CELULAS; x++)
    {
        Console.Write($"{x * FATOR_ESCALA:D2} ");
    }
    Console.WriteLine($"X");

    Console.WriteLine("-------------------------------------");
    Console.WriteLine("Legenda: B=Base | Letra=Drone | P=Pedido Pendente");
    Console.WriteLine("-------------------------------------");
}