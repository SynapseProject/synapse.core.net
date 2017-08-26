param (
    [string]$source,      #= 'C:\Devo\synapse\synapse.core.net\Synapse.Core\bin\Debug\Synapse.Core.???',
    [string]$solutionDir, #= 'C:\Devo\synapse\synapse.core.net\',
    [string]$destReplace, #= 'synapse.core.net\,synapse.server.net\packages',
    [string]$destPackage, #= 'Synapse.Core.Signed*',
    [string]$destVersion, #= '\lib\net45',
    [int]$exitError = 0
)

# take the solutionDir and replace curr folder with destination solution folder
$parts = $destReplace.Split( ',' );
$destPath = $solutionDir.Replace( $parts[0], $parts[1] );

# get the folder for the destination nuget package
$p = Get-Item $destPath | Get-ChildItem -Directory -Filter $destPackage
if( $p )
{
  $destPath = $p.FullName + $destVersion
  Copy-Item -Path $source -Destination $destPath
}
else
{
  Exit $exitError
}