; example in the slide. 2 rooms, 2 balls, 1 gripper.

(define (problem pb1)
    (:domain gripper)
  	(:objects roomA - room
                  roomB - room 
                  Ball1 - ball 
		  Ball2 - ball 
		  left - gripper)
	(:init 
		(at-robby roomA) 
		(free left) 
		(at Ball1 roomA)
		(at Ball2 roomA))
	(:goal (and (at Ball1 roomB) 
		(at Ball2 roomB)))
)
