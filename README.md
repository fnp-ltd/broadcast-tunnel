# broadcast-tunnel
Links two LANs by repeating broadcast messages

Some multiplayer games communicate with unicast TCP or UDP messages, and these can generally be easily set up to traverse firewalls and cross the public Internet.
However, other multiplayer games use network broadcast messages, and these do not traverse firewalls or even VPNs.
This project seeks to enable mutiplayer games to work between two LANs by rebroadcasting messages. You have to know the port number that's used by the game, which can be discovered using such tools as WireShark.
One side of the connection does require an incoming firewall rule to connect the two repeaters together.
