(define (problem problem)
	(:domain logistics)
	(:objects
		floor1 floor2 - floor
		person1 person2 - person
		elevator1 elevator2 - elevator
	)
	(:init
		(at-elevator elevator1 floor1)
		(at-elevator elevator2 floor2)
		(at-elevator elevator1 floor1)
		(at-person person2 floor2)
		(at-person person1 floor1)
		(at-person person1 floor1)
		(above floor1 floor2)
	)
	(:goal
		(and
			(reached
 person2)
		)
	)
)
