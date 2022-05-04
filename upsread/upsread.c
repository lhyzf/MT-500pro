/******************************************************************
 *   A simple C program to retrieve the Santak(R) UPS Data
 *
 *   INTRODUCTION:
 *   ~~~~~~~~~~~~~
 *   Since the UPS monitor software WinPower provided by
 *   Santak is based on JAVA. Such implementation wastes
 *   memory and will bring troubles while running under
 *   some legacy systems.
 *
 *   So the author wrote this *light-weight * program for
 *   anyone who needs to monitor the UPS status using
 *   shell-script.
 *
 *   This program is very simple, it just requests the UPS
 *   to provide its status info via the serial port and
 *   output the response string directly to stdout.
 *
 *   The UPS will provides the following two types of status:
 *   1) standard status
 *   ----------------
 *   Request Data: "F\r"
 *   Response:     #220.0 004 12.00
 *
 *   2) current status
 *   ----------------
 *   Request Data: "Q1\r"
 *   Response:     (228.8 228.8 228.8 011 50.1 13.8 25.0 00001001
 *   
 *   You may easily decode the means of the status info
 *   with the helps of WinPower.
 *
 *   Currently, only the following model(s) has been tested:
 *   * MT-pro UPS(500/1000VA)
 *  
 *   Author Info:
 *   Shikai Chen ( csk@live.com )
 *   http://www.csksoft.net
 *
 *   Version info:
 *   Feb.07/2009
 *         ------ First version, only supports the MT-500pro model
 *
 ******************************************************************/

#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <termios.h>

#define UPS_COM_BRATE B2400
#define UPS_COM_DEFAULT "/dev/ttyUSB0"

#define WAIT_TIME 1000
#define MAX_LOOP_TIME 200
#define MAX_DOWN_COUNT 3

//command the requires the ups to provide the standard power info
static const char *UPSCMD_STANDARD = "F\r";

//command the requires the ups to provide the current power status
static const char *UPSCMD_CURRENT = "Q1\r";

int write_n_read(
    int fd,
    const char *inbuf, unsigned int insize,
    char *outbuf, unsigned int outsize)
{
    int nread = 0;
    int nsize = 0;
    int looptime = 0;
    write(fd, inbuf, insize);

    while ((looptime++) < MAX_LOOP_TIME)
    {
        usleep(WAIT_TIME); //wait for response

        while ((nsize = read(fd, outbuf + nread, outsize - nread)) > 0)
        {
            nread += nsize;

            if (outbuf[nread - 1] == '\r')
                goto READ_FIN;
            if (nread >= outsize)
                goto READ_FIN;
        }
    }

READ_FIN:
    return nread;
}

void set_com_prop(int fd)
{
    struct termios opt;

    tcgetattr(fd, &opt);

    cfsetispeed(&opt, UPS_COM_BRATE);
    cfsetospeed(&opt, UPS_COM_BRATE);

    opt.c_cflag |= (CLOCAL | CREAD);

    opt.c_cflag &= ~PARENB;
    opt.c_cflag &= ~CSTOPB;
    opt.c_cflag &= ~CSIZE;
    opt.c_cflag |= CS8;

    opt.c_lflag &= ~(ICANON | ECHO | ECHOE | ISIG);
    opt.c_oflag &= ~OPOST;

    opt.c_cc[VTIME] = 150;
    opt.c_cc[VMIN] = 1;

    tcflush(fd, TCIFLUSH);

    tcsetattr(fd, TCSANOW, &opt);
}

int main(int argc, char *argv[])
{
    fprintf(stdout, "Start!\n");
    char recv_buff[512] = {0};
    int fd = 0;
    int recv_len = 0;

    char *target_port = UPS_COM_DEFAULT;

    //parsing argument...

    if (argc > 1)
        target_port = argv[1];

    fprintf(stdout, "%s\n", target_port);

    fd = open(target_port, O_RDWR | O_NOCTTY | O_NDELAY);

    set_com_prop(fd);
    //init connection:
    write_n_read(fd, UPSCMD_STANDARD,
                 strlen(UPSCMD_STANDARD),
                 recv_buff, sizeof(recv_buff));

    recv_len = write_n_read(fd,
                            UPSCMD_STANDARD,
                            strlen(UPSCMD_STANDARD),
                            recv_buff, sizeof(recv_buff));

    if (recv_len > 0)
    {
        recv_buff[recv_len - 1] = 0;
        fprintf(stdout, "%s\n", recv_buff);
    }

    int down_count = 0;
    int shutdown_scheduled = 0;
    while (1)
    {
        recv_len = write_n_read(fd,
                                UPSCMD_CURRENT,
                                strlen(UPSCMD_CURRENT),
                                recv_buff, sizeof(recv_buff));

        if (recv_len > 0)
        {
            recv_buff[recv_len - 1] = 0;

            fprintf(stdout, "%s\n", recv_buff);
            if (recv_buff[38] == '1')
            {
                down_count++;
                if (down_count > MAX_DOWN_COUNT
                    && !shutdown_scheduled)
                {
                    shutdown_scheduled = 1;
                    system("shutdown");
                }
            }
            else
            {
                down_count = 0;
                if (shutdown_scheduled)
                {
                    shutdown_scheduled = 0;
                    system("shutdown -c");
                    fprintf(stdout, "Shutdown canceled.\n");
                }
            }
        }
        sleep(1);
    }

    close(fd);
    return 0;
}