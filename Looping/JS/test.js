const readline = require('readline');
const { spawn } = require('child_process');

const child = spawn('dotnet', [
    'run',
    '--project', 'C:\\private\\mcp\\investmcp\\Looping\\QuickstartWeatherServer',
    '--no-build'
], {
  stdio: ['pipe', 'pipe', 'pipe'] // stdin, stdout, stderr
});

const childOutput = readline.createInterface({
  input: child.stdout,
  terminal: false
});
  
childOutput.on('line', (line) => {
  console.log(`>>>> ${line} <<<<\n`);
});

child.stdin.write(`{ "jsonrpc": "2.0", "id":1, "method": "initialize" }\n`);