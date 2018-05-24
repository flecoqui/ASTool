#!/bin/bash
# This bash file install apache
# Parameter 1 hostname 
azure_hostname=$1
astool_configfile=$2
#############################################################################
log()
{
	# If you want to enable this logging, uncomment the line below and specify your logging key 
	#curl -X POST -H "content-type:text/plain" --data-binary "$(date) | ${HOSTNAME} | $1" https://logs-01.loggly.com/inputs/${LOGGING_KEY}/tag/redis-extension,${HOSTNAME}
	echo "$1"
	echo "$1" >> /astool/log/install.log
}
#############################################################################
check_os() {
    grep ubuntu /proc/version > /dev/null 2>&1
    isubuntu=${?}
    grep centos /proc/version > /dev/null 2>&1
    iscentos=${?}
    grep redhat /proc/version > /dev/null 2>&1
    isredhat=${?}	
	if [ -f /etc/debian_version ]; then
    isdebian=0
	else
	isdebian=1	
    fi

	if [ $isubuntu -eq 0 ]; then
		OS=Ubuntu
		VER=$(lsb_release -a | grep Release: | sed  's/Release://'| sed -e 's/^[ \t]*//' | cut -d . -f 1)
	elif [ $iscentos -eq 0 ]; then
		OS=Centos
		VER=$(cat /etc/centos-release)
	elif [ $isredhat -eq 0 ]; then
		OS=RedHat
		VER=$(cat /etc/redhat-release)
	elif [ $isdebian -eq 0 ];then
		OS=Debian  # XXX or Ubuntu??
		VER=$(cat /etc/debian_version)
	else
		OS=$(uname -s)
		VER=$(uname -r)
	fi
	
	ARCH=$(uname -m | sed 's/x86_//;s/i[3-6]86/32/')

	log "OS=$OS version $VER Architecture $ARCH"
}

#############################################################################
configure_network(){
# firewall configuration 
iptables -A INPUT -p tcp --dport 80 -j ACCEPT
iptables -A INPUT -p tcp --dport 443 -j ACCEPT
}

#############################################################################
install_netcore(){
wget -q packages-microsoft-prod.deb https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt-get -y install apt-transport-https
apt-get -y update
apt-get -y install dotnet-sdk-2.1.200
}
install_netcore_centos(){
rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
yum -y update
yum -y install libunwind libicu
yum -y install dotnet-sdk-2.1.200
}
install_netcore_redhat(){
yum -y install rh-dotnet20 -y
scl enable rh-dotnet20 bash
}
install_netcore_debian(){
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
wget -q https://packages.microsoft.com/config/debian/8/prod.list
mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
apt-get update
apt-get install dotnet-sdk-2.1.200
}
#############################################################################
install_git_ubuntu(){
apt-get -y install git
}
install_git_centos(){
yum -y install git
}
#############################################################################

build_astool(){
# Download config file
cd /astool/config
wget $astool_configfile
# Download source code
cd /git
git clone https://github.com/flecoqui/ASTool.git
log "dotnet publish --self-contained -c Release -r ubuntu.16.10-x64 --output bin"
export HOME=/root
env  > /astool/log/env.log
# the generation of ASTOOL build could fail (dotnet bug)
/usr/bin/dotnet publish /git/ASTool/cs/ASTool/ASTool --self-contained -c Release -r ubuntu.16.10-x64 --output /git/ASTool/cs/ASTool/ASTool/bin > /astool/log/dotnet.log 2> /astool/log/dotneterror.log
log "dotnet publish done"

}
#############################################################################
build_astool_post(){
log "installing the service which will build astool after VM reboot"
cat <<EOF > /etc/systemd/system/buildastool.service
[Unit]
Description=build astool 

[Service]
Type=oneshot
RemainAfterExit=yes
ExecStart=/usr/bin/dotnet publish /git/ASTool/cs/ASTool/ASTool --self-contained -c Release -r ubuntu.16.10-x64 --output /git/ASTool/cs/ASTool/ASTool/bin 

[Install]
WantedBy=multi-user.target
EOF
systemctl daemon-reload
systemctl enable buildastool.service
log "Rebooting"
reboot
}

#############################################################################
install_astool(){
cd /git/ASTool/cs/ASTool/ASTool/bin
export PATH=$PATH:/git/ASTool/cs/ASTool/ASTool/bin
echo "export PATH=$PATH:/git/ASTool/cs/ASTool/ASTool/bin" >> /etc/profile

chmod +x  /git/ASTool/cs/ASTool/ASTool/bin/ASTool

adduser astool --disabled-login
cat <<EOF > /etc/systemd/system/astool.service
[Unit]
Description=astool Service
After=network.target

[Service]
Type=simple
User=astool
ExecStart=/git/ASTool/cs/ASTool/ASTool/bin/ASTool --import --configfile /astool/config/astool.linux.xml
Restart=on-abort

[Install]
WantedBy=multi-user.target
EOF
}
#############################################################################
install_astool_centos(){
cd /git/ASTool/cs/ASTool/ASTool/bin
export PATH=$PATH:/git/ASTool/cs/ASTool/ASTool/bin
echo "export PATH=$PATH:/git/ASTool/cs/ASTool/ASTool/bin" >> /etc/profile
chmod +x  /git/ASTool/cs/ASTool/ASTool/bin/ASTool
adduser astool -s /sbin/nologin
cat <<EOF > /etc/systemd/system/astool.service
[Unit]
Description=astool Service

[Service]
WorkingDirectory=/git/ASTool/cs/ASTool/ASTool/bin
User=astool
ExecStart=/git/ASTool/cs/ASTool/ASTool/bin/ASTool  --import --configfile /astool/config/astool.linux.xml'
Restart=always
RestartSec=10
SyslogIdentifier=ASTool


[Install]
WantedBy=multi-user.target
EOF
}


#############################################################################
configure_network_centos(){
# firewall configuration 
iptables -A INPUT -p tcp --dport 80 -j ACCEPT
iptables -A INPUT -p tcp --dport 443 -j ACCEPT


service firewalld start
firewall-cmd --permanent --add-port=80/tcp
firewall-cmd --permanent --add-port=443/tcp
firewall-cmd --reload
}



#############################################################################

environ=`env`
# Create folders
mkdir /git
mkdir /astool
mkdir /astool/log
mkdir /astool/config
mkdir /astool/dvr
mkdir /astool/dvr/test1
mkdir /astool/dvr/test2
# Write access in dvr subfolder
chmod -R a+rw /astool/dvr
# Write access in log subfolder
chmod -R a+rw /astool/log
log "Environment before installation: $environ"

log "Installation script start : $(date)"
log "Net Core Installation: $(date)"
log "#####  azure_hostname: $azure_hostname"
log "Installation script start : $(date)"
check_os
if [ $iscentos -ne 0 ] && [ $isredhat -ne 0 ] && [ $isubuntu -ne 0 ] && [ $isdebian -ne 0 ];
then
    log "unsupported operating system"
    exit 1 
else
	if [ $iscentos -eq 0 ] ; then
	    log "configure network centos"
		configure_network_centos
	    log "install netcore centos"
		install_netcore_centos
	    log "install git centos"
		install_git_centos
	elif [ $isredhat -eq 0 ] ; then
	    log "configure network redhat"
		configure_network_centos
	    log "install netcore redhat"
		install_netcore_redhat
	    log "install git redhat"
		install_git_centos
	elif [ $isubuntu -eq 0 ] ; then
	    log "configure network ubuntu"
		configure_network
		log "install netcore ubuntu"
		install_netcore
	    log "install git ubuntu"
		install_git_ubuntu
	elif [ $isdebian -eq 0 ] ; then
	    log "configure network"
		configure_network
		log "install netcore debian"
		install_netcore_debian
	    log "install git debian"
		install_git_ubuntu
	fi
	log "build ASTool"
	build_astool

	if [ $iscentos -eq 0 ] ; then
	    log "install astool centos"
		install_astool_centos
	elif [ $isredhat -eq 0 ] ; then
	    log "install astool redhat"
		install_astool_centos
	elif [ $isubuntu -eq 0 ] ; then
	    log "install astool ubuntu"
		install_astool
	elif [ $isdebian -eq 0 ] ; then
	    log "install astool debian"
		install_astool
	fi
	log "Start ASTOOL service"
	systemctl enable astool
	systemctl start astool 
	if [ -f /git/ASTool/cs/ASTool/ASTool/bin/ASTool ] ; then
		log "Installation successful, ASTOOL correctly generated"
	else	
		log "Installation not successful, reboot required to build ASTOOL"
		build_astool_post
	fi
fi
exit 0 

