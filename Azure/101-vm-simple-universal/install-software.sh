#!/bin/bash
# This bash file install apache
# Parameter 1 hostname 
azure_hostname=$1

#############################################################################
log()
{
	# If you want to enable this logging, uncomment the line below and specify your logging key 
	#curl -X POST -H "content-type:text/plain" --data-binary "$(date) | ${HOSTNAME} | $1" https://logs-01.loggly.com/inputs/${LOGGING_KEY}/tag/redis-extension,${HOSTNAME}
	echo "$1"
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
configure_apache(){
# Apache installation 
apt-get -y update
apt-get -y install apache2
if [ $isubuntu -eq 0 ]; then
log "Install PHP 7.0"
apt-get -y install php7.0-common libapache2-mod-php7.0 php7.0-cli
else
log "Install PHP 5"
apt-get -y install php5-common libapache2-mod-php5 php5-cli
fi
apt-get -y install curl
azure_localip=`ifconfig eth0 |  grep 'inet ' | awk '{print \$2}' | sed 's/addr://'`
azure_publicip=`curl -s checkip.dyndns.org | sed -e 's/.*Current IP Address: //' -e 's/<.*$//'`
log "Local IP address: $azure_localip"
log "Public IP address: $azure_publicip"
#
# Start Apache server
#
apachectl restart

directory=/var/www/html
if [ ! -d $directory ]; then
mkdir $directory
fi

cat <<EOF > $directory/index.php 
<html>
  <head>
    <title>Sample "Hello from $azure_hostname" </title>
  </head>
  <body bgcolor=white>

    <table border="0" cellpadding="10">
      <tr>
        <td>
          <h1>Hello from $azure_hostname</h1>
		  <p>OS $OS Version $VER Architecture $ARCH </p>
		  <p>Local IP Address: </p>

<?php
echo "$azure_localip";

?>

<p>Public IP Address: </p>
<?php
echo "$azure_publicip";

?>
        </td>
      </tr>
    </table>

    <p>This is the home page for the iperf3 test on Azure VM</p>
    <p>Launch the command line from your client: </p>
    <p>     iperf3 -c $azure_hostname -p 5201 --parallel 32  </p> 
    <ul>
      <li>To <a href="http://www.microsoft.com">Microsoft</a>
      <li>To <a href="https://portal.azure.com">Azure</a>
    </ul>
  </body>
</html>
EOF


echo "Configuring Web Site for Apache: $(date)"
cat <<EOF > /etc/apache2/conf-available/html.conf 
ServerName "$azure_hostname"
<VirtualHost *:80>
        ServerAdmin webmaster@localhost
        ServerName "$azure_hostname"

        DocumentRoot /var/www/html
        <Directory />
                Options FollowSymLinks
                AllowOverride None
        </Directory>

       # Add CORS headers for HTML5 players
        Header always set Access-Control-Allow-Headers "origin, range"
        Header always set Access-Control-Allow-Methods "GET, HEAD, OPTIONS"
        Header always set Access-Control-Allow-Origin "*"
        Header always set Access-Control-Expose-Headers "Server,range"

        # Possible values include: debug, info, notice, warn, error, crit,
        # alert, emerg.
        LogLevel warn
        LogLevel warn
        ErrorLog /var/log/apache2/azure-evaluation-error.log
        CustomLog /var/log/apache2/azure-evaluation-access.log combined
</VirtualHost>
EOF

}
#############################################################################
configure_apache_centos(){
# Apache installation 
yum clean all
yum -y install httpd
yum -y install php

azure_localip=`ifconfig eth0 |  grep 'inet ' | awk '{print \$2}' | sed 's/addr://'`
azure_publicip=`curl -s checkip.dyndns.org | sed -e 's/.*Current IP Address: //' -e 's/<.*$//'`
log "Local IP address: $azure_localip"
log "Public IP address: $azure_publicip"
#
# Start Apache server
#
systemctl start httpd
systemctl enable httpd
systemctl status httpd

directory=/var/www/html
if [ ! -d $directory ]; then
mkdir $directory
fi

cat <<EOF > $directory/index.php 
<html>
  <head>
    <title>Sample "Hello from $azure_hostname" </title>
  </head>
  <body bgcolor=white>

    <table border="0" cellpadding="10">
      <tr>
        <td>
          <h1>Hello from $azure_hostname</h1>
		  <p>OS $OS Version $VER Architecture $ARCH </p>
		  <p>Local IP Address: </p>

<?php
echo "$azure_localip";

?>

<p>Public IP Address: </p>
<?php
echo "$azure_publicip";

?>
        </td>
      </tr>
    </table>

    <p>This is the home page for the iperf3 test on Azure VM</p>
    <p>Launch the command line from your client: </p>
    <p>     iperf3 -c $azure_hostname -p 5201 --parallel 32  </p> 
    <ul>
      <li>To <a href="http://www.microsoft.com">Microsoft</a>
      <li>To <a href="https://portal.azure.com">Azure</a>
    </ul>
  </body>
</html>
EOF


echo "Configuring Web Site for Apache: $(date)"
cat <<EOF > /etc/httpd/conf.d/html.conf 
ServerName "$azure_hostname"
<VirtualHost *:80>
        ServerAdmin webmaster@localhost
        ServerName "$azure_hostname"

        DocumentRoot /var/www/html
        <Directory />
                Options FollowSymLinks
                AllowOverride None
        </Directory>

       # Add CORS headers for HTML5 players
        Header always set Access-Control-Allow-Headers "origin, range"
        Header always set Access-Control-Allow-Methods "GET, HEAD, OPTIONS"
        Header always set Access-Control-Allow-Origin "*"
        Header always set Access-Control-Expose-Headers "Server,range"

        # Possible values include: debug, info, notice, warn, error, crit,
        # alert, emerg.
        LogLevel warn
        LogLevel warn
        ErrorLog /var/log/httpd/azure-evaluation-error.log
        CustomLog /var/log/httpd/azure-evaluation-access.log combined
</VirtualHost>
EOF

}
#############################################################################
configure_network(){
# firewall configuration 
iptables -A INPUT -p tcp --dport 80 -j ACCEPT
iptables -A INPUT -p tcp --dport 443 -j ACCEPT
iptables -A INPUT -p tcp --dport 5201 -j ACCEPT
iptables -A INPUT -p udp --dport 5201 -j ACCEPT
}
#############################################################################
configure_network_centos(){
# firewall configuration 
iptables -A INPUT -p tcp --dport 80 -j ACCEPT
iptables -A INPUT -p tcp --dport 443 -j ACCEPT
iptables -A INPUT -p tcp --dport 5201 -j ACCEPT
iptables -A INPUT -p udp --dport 5201 -j ACCEPT

service firewalld start
firewall-cmd --permanent --add-port=5201/tcp
firewall-cmd --permanent --add-port=5201/udp
firewall-cmd --permanent --add-port=80/tcp
firewall-cmd --permanent --add-port=443/tcp
firewall-cmd --reload
}
#############################################################################
configure_iperf(){
#
# install iperf3
#  
apt-get remove iperf3 libiperf0
wget https://iperf.fr/download/ubuntu/libiperf0_3.1.3-1_amd64.deb
wget https://iperf.fr/download/ubuntu/iperf3_3.1.3-1_amd64.deb
dpkg -i libiperf0_3.1.3-1_amd64.deb iperf3_3.1.3-1_amd64.deb
rm libiperf0_3.1.3-1_amd64.deb iperf3_3.1.3-1_amd64.deb

adduser iperf --disabled-login
cat <<EOF > /etc/systemd/system/iperf3.service
[Unit]
Description=iperf3 Service
After=network.target

[Service]
Type=simple
User=iperf
ExecStart=/usr/bin/iperf3 -s
Restart=on-abort


[Install]
WantedBy=multi-user.target
EOF


}
#############################################################################
configure_iperf_centos(){
#
# install iperf3
#  
yum -y install libc.so.6
rpm -ivh https://iperf.fr/download/fedora/iperf3-3.1.3-1.fc24.x86_64.rpm

adduser iperf -s /sbin/nologin
cat <<EOF > /etc/systemd/system/iperf3.service
[Unit]
Description=iperf3 Service
After=network.target

[Service]
Type=simple
User=iperf
ExecStart=/usr/bin/iperf3 -s
Restart=on-abort


[Install]
WantedBy=multi-user.target
EOF
}
#############################################################################
configure_iperf_ubuntu_14(){
#
# install iperf3
#  
apt-get -y remove iperf3 libiperf0
wget https://iperf.fr/download/ubuntu/libiperf0_3.1.3-1_amd64.deb
wget https://iperf.fr/download/ubuntu/iperf3_3.1.3-1_amd64.deb
dpkg -i libiperf0_3.1.3-1_amd64.deb iperf3_3.1.3-1_amd64.deb
rm libiperf0_3.1.3-1_amd64.deb iperf3_3.1.3-1_amd64.deb

adduser iperf --disabled-login
cat <<EOF > /etc/init/iperf3.conf
# iperf3.conf
start on filesystem
script    
    /usr/bin/iperf3 -s
    echo "iperf3 started"
end script
EOF

ln -s /etc/init/iperf3.conf /etc/init.d/iperf3

}
#############################################################################
start_apache(){
apachectl restart
}
#############################################################################
start_apache_centos(){
systemctl restart httpd
systemctl enable httpd
systemctl status httpd
}
#############################################################################
start_iperf(){
systemctl enable iperf3
systemctl start iperf3  
}
#############################################################################
start_iperf_ubuntu_14(){
service iperf3 start
}
#############################################################################

environ=`env`

log "Environment before installation: $environ"

log "Installation script start : $(date)"
log "Apache Installation: $(date)"
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
	    log "configure apache centos"
		configure_apache_centos
	elif [ $isredhat -eq 0 ] ; then
	    log "configure network redhat"
		configure_network_centos
	    log "configure apache redhat"
		configure_apache_centos
	else
	    log "configure network"
		configure_network
	    log "configure apache"
		configure_apache
	fi
    if [ $isubuntu -eq 0 ] && [ "$VER" = "14" ]; then
      log "configure iperf for ubuntu 14"
      configure_iperf_ubuntu_14
      log "start iperf for ubuntu 14"
      start_iperf_ubuntu_14
    else
      log "configure iperf"
      if [ $iscentos -eq 0 ] ; then
			configure_iperf_centos
	  elif [ $isredhat -eq 0 ] ; then
			configure_iperf_centos
      else
			configure_iperf
	  fi
      log "start iperf"
      start_iperf
    fi
    log "start apache"
      if [ $iscentos -eq 0 ] ; then
	    start_apache_centos
	  elif [ $isredhat -eq 0 ] ; then
	    start_apache_centos
      else
	    start_apache
	  fi
fi
exit 0 

