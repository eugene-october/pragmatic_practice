const inputData = [
    "12:00",
    "14:29",
    "11:13pm",
    "4pm",
];

function parseTime(strTime) {
    const regex = /(?<hours>\d?\d)(:(?<minutes>\d\d))?(?<ampm>am|pm)?/;
    const parsed = strTime.match(regex);

    return parsed.groups;
}

for (const t of inputData) {
    const parsed = parseTime(t);
    console.log("parsed: ", parsed);
}
