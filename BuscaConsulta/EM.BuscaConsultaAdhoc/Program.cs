using FirebirdSql.Data.FirebirdClient;

class Program
{
    static void Main(string[] args)
    {
        //Passar via args o diretorioEntrada, o diretorioSaida. E também o data source, a Password e o port number da string de conexão
        //Exemplo: EM.BuscaConsultaAdhoc <diretorioEntrada> <diretorioSaida> <data source> <Password> <port number>

        string consulta = "SELECT CADHID, CADHNOME, CADHDEFINICAO FROM TBCONSULTAADHOC WHERE CADHDEFINICAO LIKE '%TBDUPLICATAMATRICULA%' OR CADHDEFINICAO LIKE '%TBDUPLICATAAVULSO%' OR CADHDEFINICAO LIKE '%TBDUPLICATAAVULSO%' OR CADHDEFINICAO LIKE '%TBDUPLICATACURSOSUPERIOR%'";

        string diretorioEntrada = @".\EM.BuscaConsultaAdhoc.exe C:\DADOSTESTE C:\Work\ArquivoSalvo localhost masterkey 3054:\DADOSTESTE";
        string diretorioSaida = @"C:\Work\ArquivoSalvo";

        ProcessarDiretorio(diretorioEntrada, diretorioSaida, consulta);
    }

    static void ProcessarDiretorio(string diretorioEntrada, string diretorioSaida, string consulta)
    {
        string[] arquivos = Directory.GetFiles(diretorioEntrada, "*.FB4");
        string[] subDiretorios = Directory.GetDirectories(diretorioEntrada);

        foreach (string caminhoArquivo in arquivos)
        {
            ProcessarArquivo(caminhoArquivo, diretorioSaida, consulta);
        }

        foreach (string subDiretorio in subDiretorios)
        {
            ProcessarDiretorio(subDiretorio, diretorioSaida, consulta);
        }
    }

    static void ProcessarArquivo(string caminhoArquivo, string diretorioSaida, string consulta)
    {
        string nomeArquivo = Path.GetFileNameWithoutExtension(caminhoArquivo).ToUpper();

        string stringConexao = $"initial catalog={caminhoArquivo};data source=localhost;port number=3054;Charset=ISO8859_1;User=SYSDBA;Password=masterkey;";

        using FbConnection conexao = new(stringConexao);
        conexao.Open();

        using FbCommand comando = new(consulta, conexao);
        using FbDataReader leitor = comando.ExecuteReader();

        while (leitor.Read())
        {
            string cadhID = ((string)leitor["CADHID"]);
            string cadhDefinicao = ((string)leitor["CADHDEFINICAO"]);

            cadhDefinicao = cadhDefinicao.Replace("\\r\\n", "").Replace("\\t\\t", "").Replace("\\r\\n\\t\\t\\t\\t\\t\\t\\t\\", "").Trim();

            string caminhoTxt = Path.Combine(diretorioSaida, $"{nomeArquivo}.{cadhID}.txt");
            File.WriteAllText(caminhoTxt, cadhDefinicao);
        }
    }
}
