from typing import List
import yaml
import json
import os

DATA_FOLDER = "data/"
OUTPUT_FOLDER = "out/"


def parseYAML(data: str) -> dict:
    result = yaml.safe_load(data)

    return result


def parseJSON(data: str) -> dict:
    result = json.loads(data)

    return result


def listFiles(folder: str) -> List[str]:
    files = os.listdir(folder)
    return files


def convert():
    files = listFiles(DATA_FOLDER)
    lastParsedYaml = None

    for file in files:
        path = f"{DATA_FOLDER}{file}"

        print(f"Reading {path}...")
        with open(path, "r") as f:
            content = f.read()
            parsedYaml = parseYAML(content)
            lastParsedYaml = parsedYaml

        jsonFileName = file.replace(".yml", ".json")
        outPath = f"{OUTPUT_FOLDER}{jsonFileName}"

        print(f"Producing {outPath}...")
        with open(outPath, "w") as fw:
            jsonTxt = json.dumps(lastParsedYaml)
            fw.write(jsonTxt)

        print(f"Status: SUCCESS")


def main():
    convert()


main()
