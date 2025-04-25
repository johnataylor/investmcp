const readline = require('readline');
const { spawn } = require('child_process');

const child = spawn('dotnet', [
    'run',
    '--project', 'C:\\private\\mcp\\investmcp\\Looping\\QuickstartWeatherServer',
    '--no-build'
], {
  stdio: ['pipe', 'pipe', 'pipe'] // stdin, stdout, stderr
});
 
const parent = readline.createInterface({
  input: process.stdin,
  output: process.stdout,
  terminal: false
});

parent.on('line', (line) => {

  console.error(`received ${line}`);

  child.stdin.write(`${line}\n`);
});

const childOutput = readline.createInterface({
  input: child.stdout,
  terminal: false
});
  
childOutput.on('line', (line) => {
  parent.write(`${line}\n`);
});
