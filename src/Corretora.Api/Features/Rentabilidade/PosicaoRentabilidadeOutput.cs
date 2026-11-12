namespace Corretora.Api.Features.Rentabilidade;

[GraphQLName("PosicaoRentabilidade")]
public sealed record PosicaoRentabilidadeOutput(string Ativo, decimal PercentualMes)
{
    public static PosicaoRentabilidadeOutput De(PosicaoRentabilidade posicao) =>
        new(posicao.Ativo, posicao.PercentualMes);
}
