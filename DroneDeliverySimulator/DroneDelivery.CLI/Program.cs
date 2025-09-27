using DroneDelivery.Domain.Entities;
using DroneDelivery.Domain.Enums;
using DroneDelivery.Domain.Models;
using DroneDelivery.Domain.Services;


// --------------------------------------------------------
// 1. CONFIGURAÇÃO E INICIALIZAÇÃO DA APLICAÇÃO
// --------------------------------------------------------

var calculadora = new CalculadoraEuclidiana();

// Frota inicial (você pode ajustar os valores conforme o case)
var frota = new List<Drone>
{
    new Drone("Mufasa", capacidadeMaxKg: 10.0, alcanceMaxKm: 20.0,velocidade: 6.0),
    new Drone("Simba", capacidadeMaxKg: 8.0, alcanceMaxKm: 15.0, velocidade: 6.0),
    new Drone("Timão", capacidadeMaxKg: 5.0, alcanceMaxKm: 10.0, velocidade: 6.0),
    new Drone("Pumba", capacidadeMaxKg: 18.0, alcanceMaxKm: 8.0, velocidade : 6.0)
};

var gerenciador = new GerenciadorDeFrota(calculadora, frota);
var simulador = new SimuladorDeVoos();

// Lista de pedidos que ainda não foram alocados em um voo.
var pedidosPendentes = new List<Pedido>();

// --------------------------------------------------------
// 2. FUNÇÃO PRINCIPAL: LOOP INTERATIVO
// --------------------------------------------------------
    
ExecutarInterfaceCLI();

// --------------------------------------------------------
// 3. MÉTODOS DA INTERFACE (Lógica do Menu)
// --------------------------------------------------------

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
                    return; // Sai do loop e encerra a aplicação
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

// --------------------------------------------------------
// 4. MÉTODOS AUXILIARES
// --------------------------------------------------------

void ExibirCabecalho()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("=================================================");
    Console.WriteLine("    🚁 SIMULADOR DTI - GERENCIADOR DE FROTA 🚁");
    Console.WriteLine("=================================================");
    Console.ResetColor();
    Console.WriteLine($"Frota Ativa: {frota.Count} drones ({frota.First().CapacidadeMaxKg}kg/{frota.First().AlcanceMaxKm}km, etc.)");
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
    Console.Write($"📦 Pedidos Pendentes: ");
    Console.ForegroundColor = pedidosPendentes.Count > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
    Console.Write(pedidosPendentes.Count);
    Console.ResetColor();

    Console.Write($" | ✈️ Voos em Andamento: ");
    Console.ForegroundColor = voosEmAndamento > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
    Console.Write(voosEmAndamento);
    Console.ResetColor();
    Console.WriteLine("\n-------------------------------------------------");
}

void AdicionarPedidoInterativo()
{
    Console.Clear();
    ExibirCabecalho();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\n-- NOVO PEDIDO --");
    Console.ResetColor();

    // 1. Peso
    Console.Write("Peso do Pacote (kg): ");
    if (!double.TryParse(Console.ReadLine(), out double peso) || peso <= 0)
    {
        throw new ArgumentException("O peso deve ser um número positivo.");
    }

    // 2. Coordenadas
    Console.Write("Coordenada X do Cliente: ");
    if (!double.TryParse(Console.ReadLine(), out double x))
    {
        throw new ArgumentException("Coordenada X inválida.");
    }
    Console.Write("Coordenada Y do Cliente: ");
    if (!double.TryParse(Console.ReadLine(), out double y))
    {
        throw new ArgumentException("Coordenada Y inválida.");
    }
    var localizacao = new Ponto(x, y);

    // 3. Prioridade
    Console.Write("Prioridade (A: Alta, M: Média, B: Baixa): ");
    var prioridadeInput = Console.ReadLine()?.ToUpperInvariant();
    Prioridade prioridade;

    switch (prioridadeInput)
    {
        case "A": prioridade = Prioridade.Alta; break;
        case "M": prioridade = Prioridade.Media; break;
        case "B": prioridade = Prioridade.Baixa; break;
        default: throw new ArgumentException("Prioridade inválida. Use A, M ou B.");
    }

    // Cria o objeto Pedido usando a lógica do Domain
    var novoPedido = new Pedido(localizacao, peso, prioridade);
    pedidosPendentes.Add(novoPedido);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n✅ Pedido adicionado! Peso: {peso}kg. Destino: ({x}, {y}). Pendentes: {pedidosPendentes.Count}");
    Console.ResetColor();
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

    // 1. ALOCAÇÃO (Gerenciador de Frota)
    var novosVoos = gerenciador.AlocarPedidos(pedidosPendentes);

    // Atualiza a lista de pedidos pendentes para remover os que foram alocados
    pedidosPendentes.RemoveAll(p => !gerenciador.PedidosPendentes.Contains(p));

    if (novosVoos.Any())
    {
        Console.WriteLine($"\n📦 {novosVoos.Count} novo(s) Voo(s) PLANEJADO(s) e INICIADO(s)!");
        foreach (var voo in novosVoos)
        {
            Console.WriteLine($"  -> {voo.DroneAlocado.Nome}: Peso {voo.PesoTotalCarga:F1}kg, Rota {voo.DistanciaTotalRotaKm:F1}km. {voo.Pacotes.Count} Pacotes.");
        }

        // 2. INICIAR VOOS
        simulador.IniciarVoos(novosVoos);
    }
    else if (pedidosPendentes.Any())
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n⚠️ {pedidosPendentes.Count} Pacotes ficaram pendentes. Nenhum drone disponível ou capacidade/alcance excedido.");
        Console.ResetColor();
    }

    // 3. PROCESSAR TEMPO (Simulador de Voos)
    Console.WriteLine("\nProcessando 5 minutos de simulação...");
    simulador.ProcessarCicloDeSimulacao(tempoDecorridoMinutos: 5.0);

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

    foreach (var drone in frota)
    {
        var corStatus = drone.Status == DroneStatus.Idle ? ConsoleColor.Green : ConsoleColor.Yellow;

        Console.Write($"\n{drone.Nome}: ");
        Console.ForegroundColor = corStatus;
        Console.Write(drone.Status);
        Console.ResetColor();

        if (drone.Status != DroneStatus.Idle)
        {
            Console.WriteLine($" - Localização: ({drone.LocalizacaoAtual.X}, {drone.LocalizacaoAtual.Y})");
        }
        else
        {
            Console.WriteLine(" - Pronto na Base.");
        }
    }
}