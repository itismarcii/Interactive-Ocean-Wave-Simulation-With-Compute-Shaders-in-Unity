v0.5 : Implementation of the material shader, vertex determination, floating-point

Description:    Implementation the material shader compute shader communication
                Implementation of the vertex determination and the floating-point system

Scene:          v0.5

Usage:          Similar to v0.4
                In hierarchy, Cubes (Cube, Cube (1), Cube (2) are generated floating objects
                To generate own floating-object follow instructions:
                
                    1. Create GameObject that is the floating object
                    2. Attach Rigidbody and deactivate gravity
                    3. Add empty GameObjects as children to the floating object (floater)
                    4. Attach Floater script
                    5. Attach the Rigidbody from the parent to the Floater script
                    6. Drag the Floater objects to the desired positions
                    7. Fill parameters                      

