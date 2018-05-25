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

download_astool(){

# Install pre-requisites
apt-get -y install apt-transport-https
apt-get -y update

apt-get -y install libunwind8 
apt-get -y install libuuid1 

apt-get -y install liblttng-ust0
apt-get -y install libcurl3
apt-get -y install libssl1.0.0
apt-get -y install libkrb5-3
apt-get -y install zlib1g
apt-get -y install libicu52 
apt-get -y install libicu55 
apt-get -y install libicu57 
apt-get -y install libicu60 
apt-get -y install libc-bin 


# Download config file
cd /astool/config
wget $astool_configfile

# Download astool binary
cd /astool/release
wget https://github.com/flecoqui/ASTool/raw/master/Releases/LatestRelease.ubuntu.tar.gz
tar  -xzvf LatestRelease.ubuntu.tar.gz

}
#############################################################################

download_astool_centos(){

# Install pre-requisites
yum -y install libunwind libicu
yum -y install libuuid

yum -y install lttng-ust
yum -y install libcurl
yum -y install openssl-libs
yum -y install krb5-libs
yum -y install libicu
yum -y install zlib


# Download config file
cd /astool/config
wget $astool_configfile

# Download astool binary
cd /astool/release
wget https://github.com/flecoqui/ASTool/raw/master/Releases/LatestRelease.centos.tar.gz
tar  -xzvf LatestRelease.centos.tar.gz

}
#############################################################################

download_astool_redhat(){

# Install pre-requisites
yum -y install libunwind libicu
yum -y install libuuid

yum -y install lttng-ust
yum -y install libcurl
yum -y install openssl-libs
yum -y install krb5-libs
yum -y install libicu
yum -y install zlib


# Download config file
cd /astool/config
wget $astool_configfile

# Download astool binary
cd /astool/release
wget https://github.com/flecoqui/ASTool/raw/master/Releases/LatestRelease.rhel.tar.gz
tar  -xzvf LatestRelease.rhel.tar.gz

}
#############################################################################

download_astool_debian(){

# Install pre-requisites
apt-get -y install apt-transport-https
apt-get -y update

apt-get -y install libunwind8 
apt-get -y install libuuid1 

apt-get -y install liblttng-ust0
apt-get -y install libcurl3
apt-get -y install libssl1.0.0
apt-get -y install libkrb5-3
apt-get -y install zlib1g
apt-get -y install libicu52 
apt-get -y install libicu55 
apt-get -y install libicu57 
apt-get -y install libicu60 
apt-get -y install libc-bin 


# Download config file
cd /astool/config
wget $astool_configfile

# Download astool binary
cd /astool/release
wget https://github.com/flecoqui/ASTool/raw/master/Releases/LatestRelease.debian.tar.gz
tar  -xzvf LatestRelease.debian.tar.gz

}


#############################################################################
install_astool(){
cd /astool/release/publish
export PATH=$PATH:/astool/release/publish
echo "export PATH=$PATH:/astool/release/publish" >> /etc/profile

chmod +x  /astool/release/publish/ASTool

adduser astool --disabled-login
cat <<EOF > /etc/systemd/system/astool.service
[Unit]
Description=astool Service
After=network.target

[Service]
Type=simple
User=astool
ExecStart=/astool/release/publish/ASTool --import --configfile /astool/config/astool.linux.xml
Restart=on-abort

[Install]
WantedBy=multi-user.target
EOF
}
#############################################################################
install_astool_centos(){
cd /astool/release/publish
export PATH=$PATH:/astool/release/publish
echo "export PATH=$PATH:/astool/release/publish" >> /etc/profile
chmod +x  /astool/release/publish/ASTool
adduser astool -s /sbin/nologin
cat <<EOF > /etc/systemd/system/astool.service
[Unit]
Description=astool Service

[Service]
WorkingDirectory=/astool/release/publish
User=astool
ExecStart=/astool/release/publish/ASTool  --import --configfile /astool/config/astool.linux.xml'
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
mkdir /astool
mkdir /astool/release
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
		log "Download ASTool centos"
		download_astool_centos
	elif [ $isredhat -eq 0 ] ; then
	    log "configure network redhat"
		configure_network_centos
		log "Download ASTool redhat"
		download_astool_redhat
	elif [ $isubuntu -eq 0 ] ; then
	    log "configure network ubuntu"
		configure_network
		log "Download ASTool ubuntu"
		download_astool
	elif [ $isdebian -eq 0 ] ; then
	    log "configure network"
		configure_network
		log "Download ASTool debian"
		download_astool_debian
	fi
	log "Download ASTool"
	download_astool

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
fi
exit 0 

