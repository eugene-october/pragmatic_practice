from parsimonious.grammar import Grammar

grammar = Grammar(
    r"""
    expr   = term ("+" term)*
    term   = factor ("*" factor)*
    factor = number / "(" expr ")"
    number = ~r"[0-9]"
"""
)
parsed = grammar.parse("2+3*4")
print("parsed: ", parsed)
