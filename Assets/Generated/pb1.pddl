(define (problem problem)
	(:domain logistics)
	(:objects
		room1 room2 - room
		ball1 - ball
		robot1 - robot
	)
	(:init
		(at-robby robot1 room1)
		(at ball1 room1)
		(connected room1 room2)
	)
	(:goal
		(and
		)
	)
)
