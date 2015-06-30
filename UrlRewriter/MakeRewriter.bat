@Echo Off

C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\csc /target:library /out:bin/ASPDNSF.URLRewriter.dll rewriter.cs assemblyinfo.cs
Echo Compilation Complete
pause