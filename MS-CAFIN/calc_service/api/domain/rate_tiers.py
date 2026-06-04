def normalize_rate_tiers(tiers: list[dict] | None) -> list[dict]:
    """Ordena faixas por from_period e garante estrutura mínima."""
    if not tiers:
        return []
    return sorted(
        [{'from_period': int(t['from_period']), 'rate': float(t['rate'])} for t in tiers],
        key=lambda t: t['from_period'],
    )


def rate_for_period(period: int, default_rate: float, tiers: list[dict]) -> float:
    """Taxa aplicável no período (1-based)."""
    if not tiers:
        return default_rate
    applicable = default_rate
    for tier in tiers:
        if period >= tier['from_period']:
            applicable = tier['rate']
    return applicable


def validate_rate_tiers(tiers: list[dict], time: float) -> None:
    if not tiers:
        return
    if tiers[0]['from_period'] != 1:
        raise ValueError('A primeira faixa de taxa deve começar no período 1.')
    periods = {t['from_period'] for t in tiers}
    if len(periods) != len(tiers):
        raise ValueError('Cada faixa deve ter um período inicial único.')
    max_period = int(time)
    for t in tiers:
        if t['from_period'] > max_period:
            raise ValueError(
                f'O período inicial {t["from_period"]} não pode ser superior ao número total de períodos ({max_period}).'
            )
