(define (problem problem)
	(:domain logistics)
	(:objects
		floor1 floor2 - floor
		person1 person2 - person
		elevator1 elevator2 - elevator
	)
	(:init
		(at-person person1 floor1)
		(at-elevator elevator1 floor1)
		(target person1 floor1)
		(in elevator1 person1)
		(above floor1 floor1)
		(full elevator1)
	)
	(:goal
		(and
			(reached person1)
		)
	)
)
