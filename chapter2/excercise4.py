from typing import Tuple


program = input("Command: ")

def parseProgram(program: str) -> Tuple[str, str]:
    return program.split(' ')

class BaseCommand:
    def performCommand(args):
        pass

class NoArgCommand:
    pass

class PenSelectCommand(BaseCommand):
    pass

class PenDownCommand(BaseCommand, NoArgCommand):
    pass

class PenUpCommand(BaseCommand, NoArgCommand):
    pass

class DrawWestCommand(BaseCommand):
    pass

class DrawNorthCommand(BaseCommand):
    pass

class DrawEastCommand(BaseCommand):
    pass

class DrawSouthCommand(BaseCommand):
    pass

def GetCommandFactory(command_id: str):
    if command_id == "P":
        return PenSelectCommand
    if command_id == "D":
        return PenDownCommand
    if command_id == "U":
        return PenUpCommand
    if command_id == "W":
        return DrawWestCommand
    if command_id == "N":
        return DrawNorthCommand
    if command_id == "E":
        return DrawEastCommand
    if command_id == "S":
        return DrawSouthCommand

    raise Exception("Unsupported command")

class ArgParser:
    @static
    def parse(arg):
        if not arg:
            return None

        return int(arg)

(commandId, args) = parseProgram(program)

try:
    command = GetCommandFactory(commandId)
    arg = ArgParser.parse(args)

    if (isinstance(command, NoArgsCommand)):
        return command.performCommand()

    command.performCommand(arg)
except:
    print("ERROR: FATAL ERROR")






# Internal language
p = Pen()
p.select(2)
p.down()
p.drawWest(2)
p.drawNorth(1)
p.drawEast(2)
p.drawSouth(1)
p.up()
