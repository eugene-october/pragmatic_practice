import { readFileSync, writeFileSync, readdirSync } from 'fs';
import path from 'node:path';
import { parseArgs } from './argsParse.js';

const INPUT_FOLDER = "data/";
const CAMEL_CASE_REGEXP = /[a-z][A-Z]/g;

const log = console.log;

const args = process.argv.slice(2);

const files = readdirSync(INPUT_FOLDER);
const filesFullPath = files.map((f) => {
    const result = path.join(INPUT_FOLDER, f);

    return result;
});

// for prod nodejs parseArgs() should be used instead. Custom implementation just for practice
var parsedArgs = parseArgs(args);
log("parsedArgs: ", parsedArgs);

filesFullPath.forEach((file, i) => {
    if (!parsedArgs?.matcher?.test(file)) {
        log("IGNORED: ", file);

        return;
    }

    log("PARSING: ", file);

    const content = readFileSync(file).toString("utf8");

    const fixedContent = content.split("\n").map((line) => {
        const newLine = line.replaceAll(/[a-z][A-Z]/g, (match) => {
            const fixedMatch = match.toLowerCase().split("");
            fixedMatch.splice(1, 0, "_");

            return fixedMatch.join("");
        });

        return newLine;
    }).join("\n");


    if (!parsedArgs?.shouldFix) {
        log("PERFORMING DRY RUN. NO CHANGES");

        log("\n\n\ncontent: ", content);
        log("\n\n\nfixedContent: ", fixedContent);
        return;
    }

    log("Writing backup... ");
    const backupFileName = `${file}.backup`;
    writeFileSync(backupFileName, content);
    log(`Successfully written: ${backupFileName}`);

    log("Rewriting original file... ");
    writeFileSync(file, fixedContent);
    log(`Successfully written: ${file}`);

    // No report needed anymore
    // report(content);
});

function report(fileContent) {
    // log("content: ", content);
    const report = [];
    content.split('\n').forEach((line, lineIdx) => {
        const matches = line.match(CAMEL_CASE_REGEXP)

        if (!matches) {
            return;
        }

        matches.forEach((m) => {
            let idx = 0;

            while (idx != -1) {
                idx = line.indexOf(m, idx);

                if (idx === -1) {
                    return;
                }

                report.push({
                    line: lineIdx,
                    position: idx,
                    occurence: m
                });

                idx += 1;
            }
        });
    });
}
