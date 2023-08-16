
using FirebirdSql.Data.FirebirdClient;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 5)
        {
            Console.WriteLine("EM.BuscaConsultaAdhoc <diretorioEntrada> <diretorioSaida> <data source> <Password> <port number>");
            return;
        }

        string diretorioEntrada = args[0];
        string diretorioSaida = args[1];
        string dataSource = args[2];
        string password = args[3];
        int portNumber = int.Parse(args[4]);

        string consulta = "SELECT CADHID, CADHNOME, CADHDEFINICAO FROM TBCONSULTAADHOC WHERE CADHDEFINICAO LIKE '%TBDUPLICATAMATRICULA%' OR CADHDEFINICAO LIKE '%TBDUPLICATAAVULSO%' OR CADHDEFINICAO LIKE '%TBDUPLICATAAVULSO%' OR CADHDEFINICAO LIKE '%TBDUPLICATACURSOSUPERIOR%'";

        ProcessarDiretorio(diretorioEntrada, diretorioSaida, consulta, dataSource, password, portNumber);
    }

    static void ProcessarDiretorio(string diretorioEntrada, string diretorioSaida, string consulta, string dataSource, string password, int portNumber)
    {
        string[] arquivos = Directory.GetFiles(diretorioEntrada, "*.FB4");
        string[] subDiretorios = Directory.GetDirectories(diretorioEntrada, "*", SearchOption.AllDirectories);

        foreach (string caminhoArquivo in arquivos)
        {
            ProcessarArquivo(caminhoArquivo, diretorioSaida, consulta, dataSource, password, portNumber);
        }

        foreach (string subDiretorio in subDiretorios)
        {
            ProcessarDiretorio(subDiretorio, diretorioSaida, consulta, dataSource, password, portNumber);
        }
    }

    static void ProcessarArquivo(string caminhoArquivo, string diretorioSaida, string consulta, string dataSource, string password, int portNumber)
    {
        string nomeArquivo = Path.GetFileNameWithoutExtension(caminhoArquivo).ToUpper();

        string stringConexao = $"initial catalog={caminhoArquivo};data source={dataSource};port number={portNumber};Charset=ISO8859_1;User=SYSDBA;Password={password};";

        using FbConnection conexao = new(stringConexao);
        conexao.Open();

        using FbCommand comando = new(consulta, conexao);
        using FbDataReader leitor = comando.ExecuteReader();
        string nomeArquivoSaida = $"{nomeArquivo}.txt";
        string caminhoTxt = Path.Combine(diretorioSaida, nomeArquivoSaida);

        using StreamWriter writer = new(caminhoTxt);
        while (leitor.Read())
        {
            string cadhID = ((string)leitor["CADHID"]);
            string cadhDefinicao = ((string)leitor["CADHDEFINICAO"]);

            cadhDefinicao = cadhDefinicao.Replace("\\r\\n", "").Replace("\\t\\t", "").Replace("\\r\\n\\t\\t\\t\\t\\t\\t\\t\\", "").Trim();

            writer.WriteLine($"Cadh ID: {cadhID}");
            writer.WriteLine($"Definição: {cadhDefinicao}");
            writer.WriteLine();
        }

        if (writer.BaseStream.Length == 0)
        {
            writer.Close();
            File.Delete(caminhoTxt);
        }
    }
}
