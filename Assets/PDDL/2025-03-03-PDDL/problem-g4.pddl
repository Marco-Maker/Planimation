; example in the slide. 2 rooms, 3 balls, 1 robot, 2 grippers.

(define (problem pb1)
    (:domain gripper)
  	(:objects roomA - room
                  roomB - room 
                  Ball1 - ball 
		  Ball2 - ball 
		  Ball3 - ball
		  left - gripper
		  right - gripper)
	(:init 
		(at-robby roomA) 
		(free left) 
	        (free right)
		(at Ball1 roomA)
		(at Ball2 roomA)
		(at Ball3 roomA))
	(:goal (and (at Ball1 roomB) 
		(at Ball2 roomB)
		(at Ball3 roomB)))
)
