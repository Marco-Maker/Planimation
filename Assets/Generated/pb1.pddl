(define (problem problem)
	(:domain logistics)
	(:objects
		room1 room2 room3 room4 room5 room6 room7 room8 - room
	)
	(:init
		(connected room1 room2)
		(connected room1 room3)
		(connected room1 room4)
		(connected room2 room5)
		(connected room3 room6)
		(connected room4 room8)
	)
	(:goal
		(and
		)
	)
)
