module Utility

open System.Reflection
open System.IO

let public GetToken(key: string) = 
  let assembly = Assembly.GetExecutingAssembly()
  let stream = assembly.GetManifestResourceStream("Utility..env")
  let env = (new StreamReader(stream)).ReadToEnd()

  let index = env.IndexOf(key)
  let startIndex = env.IndexOf('\"', index) + 1;
  let endIndex = env.IndexOf('\"', startIndex);
  env[startIndex..endIndex - 1];