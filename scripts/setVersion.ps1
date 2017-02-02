$path = "c:\Users\$($env:username)\desktop\AssemblyInfo.cs" 
$assm = Get-Content $path
$vers = $assm -match 'AssemblyFileVersion\( \"(?<v>.*)\" \)'
$vers = ([regex]::Match( $assm, 'AssemblyFileVersion\( \"(?<v>.*)\" \)' )).Groups
Write-Host $vers.Groups[1]
$version = $vers.Groups[1].Value
[Version]$currVer = [Version]$version
#$foo.Revision = $foo.Revision + 1
#Write-Host $foo

$now = [DateTime]::Now
$newBuild = '{0}{1}' -f $now.ToString( 'yy' ), $now.DayOfYear.ToString( 'D3' )
[int]$newRevision = [int]$currVer.Revision
if( $currVer.Build.ToString() -eq $newBuild )
{
    $newRevision = [int]$currVer.Revision + 1
}

#[int]$newRevision = [int]$currVer.Revision + 1
$v = '{0}.{1}.{2}.{3}' -f $currVer.Major, $currVer.Minor, $newBuild, $newRevision
[Version]$newVer = [Version]$v
Write-Host $v