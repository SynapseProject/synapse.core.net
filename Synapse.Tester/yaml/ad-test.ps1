$data = [System.Text.Encoding]::ASCII.GetString([System.Convert]::FromBase64String($args[0]))

$obj = ConvertFrom-Json -InputObject $data



write-host ">> Group Name : $($obj.GroupName)"

foreach ($user in $($obj.Users)) {

	write-host "  >> User : $user"

}



exit 0
