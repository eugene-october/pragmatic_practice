from parsimonious.grammar import Grammar
from parsimonious.nodes import NodeVisitor

MINUTES_PER_HOUR = 60

class MyVisitor(NodeVisitor):
    def __init__(self):
        self.totalminutes = 0

    def visit_expr(self, node, visited_children):
        """ Returns the overall output. """
        print("visit_expr")
        output = {}
        for child in visited_children:
            output.update(child[0])
        return output

    def visit_entry(self, node, visited_children):
        """ Makes a dict of the section (as key) and the key/value pairs. """
        print("visit_entry")
        key, values = visited_children
        return {key: dict(values)}

    def visit_section(self, node, visited_children):
        """ Gets the section name. """
        print("visit_section")
        _, section, *_ = visited_children
        return section.text

    def visit_pair(self, node, visited_children):
        """ Gets each key/value pair, returns a tuple. """
        print("visit_pair")
        key, _, value, *_ = node.children
        return key.text, value.text

    def generic_visit(self, node, visited_children):
        """ The generic visit method. """
        # Verbose logs
        # print("\n\n\n\ngeneric_visit")
        # print("node: ", node)

        expr_name = node.expr_name
        if expr_name == "shorthours":
            self.totalminutes += int(node.text) * MINUTES_PER_HOUR

        if expr_name == "fullhours":
            self.totalminutes += int(node.text) * MINUTES_PER_HOUR

        if expr_name == "minutes":
            self.totalminutes += int(node.text)

        if expr_name == "ampm":
            if node.text == "pm":
                self.totalminutes += 12 * MINUTES_PER_HOUR

        return visited_children or node


with open("../excercise5_3.peg", "r") as file:
    content = file.read()
    # Verbose logs
    # print("content", content)

    # grammar = Grammar(
    #     r"""
    #     nonzerodigit = "1"
    #        / "2"
    #        / "3"
    #        / "4"
    #        / "5"
    #        / "6"
    #        / "7"
    #        / "8"
    #        / "9"
    #     """
    # )
    # parsed = grammar.parse("1")
    # print("parsed: ", parsed)

    # Works with PEG grammar
    grammar = Grammar(content)
    dataset = (
            "12:00",
            "14:29",
            "11:13pm",
            "4pm",
            )

    for time in dataset:
        print("time: ", time)
        tree = grammar.parse(time)
        v = MyVisitor()
        v.visit(tree)
        print("v.totalminutes", v.totalminutes)
