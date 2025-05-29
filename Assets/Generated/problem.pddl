(define (problem problem)
	(:domain domain-robot-normal)
	(:objects
		room1 room2 - room
		ball1 - ball
		robot1 - robot
	)
	(:init
		(at )
		(connected room1 room1)
		(allowed robot1 room1)
		(carry ball1 robot1)
		(free robot1)
	)
	(:goal
		(and
			(at Option A Option A)
		)
	)
)
