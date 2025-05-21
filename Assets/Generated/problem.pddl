(define (problem problem)
	(:domain )
	(:objects
		floor1 floor2 - floor
		person1 - person
		elevator1 - elevator
	)
	(:init
		(above floor2 floor1)
		(target person1 floor2)
		(at-elevator elevator1 floor1)
		(at-person person1 floor1)
	)
	(:goal
		(and
			(reached person1)
		)
	)
)
