# Button2Key
Simple mapping utility to enable mapping of a DirectInput compatible controller to simulated keyboard input.

I created this app as I needed a simple way to map some of my Logitech G29 steering wheel buttons to specific keys in Assetto Corsa.

For example, entering the following commands (in sequence) will map L2 to reset the VR camera.  (This saves you from having to take the headset off and press CTRL+Space)


    add type=keyboardinput name=assettocorsa_resetvrcam description="Reset VR Camera" command="keydown:control;keydown:space;sleep:100;keyup:space;keyup:control"
    
    add type=mapping button=Buttons7 value="equals:128" input=assettocorsa_resetvrcam
    
    listen productguid=c24f046d-0000-0000-0000-504944564944
